using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Koinonia;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{
    public class KoinoniaApplication : IInstallRegistryProvider, IDownloadablesHostsRegistryProvider
    {
        private KoinoniaApplication()
        {
        }

        private IGithubApiRequestManager _githubApiRequestManager;
        private KoinoniaUnityCli _unityCliServer;
        private List<Install> _installsRegistry;
        private List<Assembly> _allAssemblies;

        public static KoinoniaApplication Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KoinoniaApplication();
                    _instance.FinalizeInstalls();
                }
                return _instance;
            }
            set { _instance = value; }
        }

        public List<DownloadablesHost> DownloadablesHostsRegistry { get; set; }

        public List<Install> InstallsRegistry
        {
            get { return _installsRegistry ?? (_installsRegistry = new List<Install>()); }
            set { _installsRegistry = value; }
        }

        public IGithubApiRequestManager GithubApiRequestManager
        {
            get
            {
                return _githubApiRequestManager ??
                       (_githubApiRequestManager = new UnityGithubApiRequestManager(AccessToken));
            }
        }

        public static string AccessToken
        {
            get
            {
                return ThreadingUtils.GetOnMainThread(() => EditorPrefs.GetString("KOINONIA_GITHUBKEY", "")); 
            }
            set
            {
                ThreadingUtils.WaitOnMainThread(() => EditorPrefs.SetString("KOINONIA_GITHUBKEY", value));
            }
        }

        /*
        public void CreateOrEditLocalPackageConfig(CreateOrEditLocalPackageConfigModel model)
        {
            PackageConfigManager.Name = model.Name;
            PackageConfigManager.Version = model.Version;
            PackageConfigManager.License = model.License;
            PackageConfigManager.Save();
        }
        */

        private static IPackageConfigManager PackageConfigManager
        {
            get {
                return _packageConfigManager ??
                       (_packageConfigManager =
                           new PackageConfigManager(new LocalPackageConfigRepository(PathUtils.PackageConfigurationPath))); }
        }

        public IKoinoniaLogger Logger
        {
            get { return UnityCliServer; }
        }

        public ITerminalFrontend CliFrontend
        {
            get { return UnityCliServer; }
        }

        public IPackageConfigManager LocalConfig
        {
            get { return PackageConfigManager; }
        }

        public void FinalizeInstalls()
        {
            this.FetchInstallsRegistry();
            foreach (var install in InstallsRegistry)
            {
                if (!install.InstallFinalized)
                {
                    install.InstallFinalized = TryFinalizeInstall(install);
                }
            }
            this.CommitInstallRegistry();
        }


        public void TestInstaller(string installCode, string installerCode)
        {

            var installScheme = GithubSchemeDecoder.DecodeShort(installCode);
            this.FetchInstallsRegistry();
            var install = Installs.FirstOrDefault(_ => _.RepositoryName == installScheme.Name && _.AuthorName == installScheme.Owner);

            if (install == null)
            {
                Logger.LogProblem("No install found matching: "+installCode);
                return;
            }

            var regex = new Regex("(?<type>[^\\.]*)\\.(?<method>[^\\.]*)");

            var mathc = regex.Match(installerCode);

            if (!mathc.Success)
            {
                Logger.LogProblem("Code should be in format: <InstallerType>.<Method>");
                return;
            }

            var typeName = mathc.Groups["type"].Value;
            var methodName = mathc.Groups["method"].Value;

            var type =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(_ => _.GetType(typeName))
                    .FirstOrDefault(_ => _ != null);

            if (type == null)
            {
                Logger.LogProblem("Installer type not found: "+typeName);
                return;
            }

            var method = type.GetMethod(methodName);

            if (method == null)
            {
                Logger.LogProblem("Method not found: " + methodName);
                return;
            }

            var result = false;

            var installer = (Installer)Activator.CreateInstance(type, new object[] { CliFrontend });

            result = (bool)method.Invoke(installer, new object[] { install });

            if (result)
            {
                Logger.Log("Install Test Finished");
            }
            else
            {
                Logger.Log("Install Not Finished");
            }
        }

        public bool TryFinalizeInstall(Install install)
        {
            Logger.Log("Finalizing "+install.ToShortString());

            if (!File.Exists(install.GetInstallerPath()))
            {
                Logger.Log("No installer found for " + install.ToShortString() + " - This is normal.");
                Logger.Log("Considering "+install.ToShortString()+" installed." );
                return true; //Nothing to install
            }

            Logger.Log("Found installer at "+install.GetInstallerPath());
            Logger.Log("Compiling now...");

            var installerCode = File.ReadAllText(install.GetInstallerPath());

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            // True - memory generation, false - external file generation
            parameters.GenerateInMemory = true;
            // True - exe file generation, false - dll file generation
            parameters.GenerateExecutable = false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToArray();
            foreach (var assembly in assemblies)
            {

                parameters.ReferencedAssemblies.Add(assembly.Location);
            }

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, installerCode);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText)); 

                }



                throw new InvalidOperationException(sb.ToString());
            }

            Logger.Log("Compiled! Executing...");

            Assembly resultAssembly = results.CompiledAssembly;
            Type program = resultAssembly.GetTypes().FirstOrDefault(t => typeof(Installer).IsAssignableFrom(t));

            var installer = (Installer)Activator.CreateInstance(program, new object[] { CliFrontend });

            MethodInfo main = program.GetMethod("PostInstall");

            return (bool)main.Invoke(installer, new object[] {install});
        }


        public void InstallNode(Downloadable downloadable)
        {
            var installation = new Installation(this,this, GithubApiRequestManager, Logger);
            var result = installation.Install(downloadable);

            if (result.Success)
            {
                var reimports = Installs.Where(i => i.ConfigData.RequiresFullReimport).ToArray();



                if (reimports.Any())
                {

                    var msg = "Following packages require full reimport to function properly:\n";
                    foreach (var reimport in reimports)
                    {
                        msg += reimport.ToShortString() + "\n";
                    }

                    ThreadingUtils.DispatchOnMainThread(() =>
                    {
                        if (EditorUtility.DisplayDialog("Koinonia", msg, "Ok", "No, I'll do it myself"))
                        {
                            EditorApplication.ExecuteMenuItem("Assets/Reimport All");
                        }
                        else
                        {
                            AssetDatabase.Refresh();
                        }
                    });

                }
                else
                {
                    ThreadingUtils.DispatchOnMainThread(AssetDatabase.Refresh);

                }

            }
        }
        
        public void UninstallNode(Install selectedPackage)
        {
           // FileUtils.DeleteFolder(new DirectoryInfo(selectedPackage.Path));
            this.Commit();
            AssetDatabase.Refresh();
        }

        public static bool LogginEnabled = true;

        private static IPackageConfigManager _packageConfigManager;

        private static string _accessToken;
        private static KoinoniaApplication _instance;

        public KoinoniaUnityCli UnityCliServer
        {
            get { return _unityCliServer ?? (_unityCliServer = new KoinoniaUnityCli()); }
            set { _unityCliServer = value; }
        }

        public static void Log(string msg)
        {
            if (!LogginEnabled) return;
            Debug.Log(msg);
        }

        public static void LogError(Exception ex)
        {
            Debug.LogError(ex);
        }

        public IEnumerable<Install> Installs
        {
            get
            {
                return this.FetchInstallsRegistry().InstallsRegistry;
            }
        }

        public void AddInstall(Install inst)
        {
            InstallsRegistry.Add(inst);
        }

        public void RemoveInstall(Install inst)
        {
            InstallsRegistry.Remove(inst);
        }

        public void Commit()
        {
            this.CommitInstallRegistry();
        }

        public IEnumerable<DownloadablesHost> DownloadablesHosts
        {
            get
            {
                return this.FetchDownloadableHostsRegistry().DownloadablesHostsRegistry;
            }
        }
    }

    public interface IKoinoniaLogger
    {
        void Log(string str);
        void LogProblem(string error);
        void LogWarning(string warning);
    }

    public interface IDownloadablesHostsRegistryProvider
    {
        IEnumerable<DownloadablesHost> DownloadablesHosts { get; } 
    }

    public interface IInstallRegistryProvider
    {
        IEnumerable<Install> Installs { get; }
        void AddInstall(Install inst);
        void RemoveInstall(Install inst);
        void Commit();
    }

    public static class KoinoniaApplicationExtensions
    {

        public static IGithubApiRequestManager GithubApiManager
        {
            get { return KoinoniaApplication.Instance.GithubApiRequestManager; }
        }

        public static KoinoniaApplication FetchDownloadableHostsRegistry(this KoinoniaApplication koinonia, bool force = false)
        {

            if (koinonia.DownloadablesHostsRegistry != null && !force) return koinonia;

            var registry =
                GithubApiManager.GetGithubRepositoriesRegistry(
                    "https://gist.githubusercontent.com/nitreo/7e340786ef4b1212416694dd10362339/raw");

            koinonia.DownloadablesHostsRegistry = registry
                .Select(_ => new DownloadablesHost()
                {
                    AuthorName = _.AuthorName,
                    RepositoryName = _.RepositoryName
                }).ToList();

            return koinonia;
        }

        public static KoinoniaApplication FetchInstallsRegistry(this KoinoniaApplication koinonia, bool force = false)
        {

            var configPath = PathUtils.InstallRegistryPath;

            if (!File.Exists(configPath)) return koinonia;

            var json = File.ReadAllText(configPath);

            koinonia.InstallsRegistry = JsonUtils.DeserializeObject<List<Install>>(json);

            return koinonia;
        }

        public static KoinoniaApplication CommitInstallRegistry(this KoinoniaApplication koinonia)
        {
            var configPath = PathUtils.InstallRegistryPath;

            FileUtils.EnsureFolderFor(configPath);

            if(!File.Exists(configPath)) File.Create(configPath).Dispose();

            File.WriteAllText(configPath,JsonUtils.SerializeObject(koinonia.InstallsRegistry).ToString());

            return koinonia;
        }

    }

}
