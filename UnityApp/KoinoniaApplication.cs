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

        private IGithubApi _githubApi;
        private UnityTerminalFrontend _unityTerminalFrontendServer;
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

        public IGithubApi GithubApi
        {
            get
            {
                return _githubApi ??
                       (_githubApi = new GithubApi(new WebRequestManager(AccessToken),new WebRequestManager(null) ));
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

        public IKoinoniaLogger Logger
        {
            get { return UnityTerminalFrontendServer; }
        }

        public ITerminalFrontend CliFrontend
        {
            get { return UnityTerminalFrontendServer; }
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
            var installation = new Installation(this,this, GithubApi, Logger);
            var result = installation.Install(downloadable);

            if (result.Success)
            {
                var reimports = Installs.Where(i => i.ConfigData.RequiresFullReimport && !i.InstallFinalized).ToArray();

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
            var installation = new Uninstallation(this, this, GithubApi, Logger);
            var result = installation.Uninstall(selectedPackage);

            if (selectedPackage.ConfigData.RequiresFullReimport)
            {

                var msg = "Deinstalled package requires full reimport "+selectedPackage.ToShortString();

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

        public static bool LogginEnabled = true;


        private static string _accessToken;
        private static KoinoniaApplication _instance;

        public UnityTerminalFrontend UnityTerminalFrontendServer
        {
            get { return _unityTerminalFrontendServer ?? (_unityTerminalFrontendServer = new UnityTerminalFrontend()); }
            set { _unityTerminalFrontendServer = value; }
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

        public void Update(Install install)
        {

            var update = new Updater(this,this,GithubApi,Logger);
            update.Update(install);


            var reimports = Installs.Where(i => i.ConfigData.RequiresFullReimport && !i.InstallFinalized).ToArray();

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
}
