using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Koinonia
{
    public class InstallationResult
    {
        public bool Success { get; set; }
    }

    public class Installation
    {
        public Installation(IDownloadablesHostsRegistryProvider hostsRegistry, IInstallRegistryProvider installRegistry, IGithubApiRequestManager githubApi, IKoinoniaLogger logger)
        {
            _hostsRegistry = hostsRegistry;
            _installsRegistry = installRegistry;
            _githubApi = githubApi;
            _logger = logger;
        }

        private IDownloadablesHostsRegistryProvider _hostsRegistry;

        private List<InstallPlanEntry> _planInstall;
        private List<InstallPlanEntry> PlanInstall
        {
            get { return _planInstall ?? (_planInstall = new List<InstallPlanEntry>()); }
            set { _planInstall = value; }
        }

        private InstallPlanEntry Add(Downloadable downloadable)
        {
            _logger.Log("Adding "+downloadable+" to the queue");

            if (downloadable.ConfigData == null)
            {
                _logger.Log("Fetching config file for "+downloadable);
                downloadable.FetchConfigData();
            }

            if (downloadable.ConfigData == null)
            {
                throw new ConfigDataNotFoundException(downloadable);
            }

            var installationEntry = new InstallPlanEntry()
            {
                AuthorName = downloadable.AuthorName,
                RepositoryName = downloadable.RepositoryName,
                CommitSha = downloadable.CommitSha,
                Name = downloadable.Name,
                Type = downloadable.Type,
                ConfigData = downloadable.ConfigData
            };
            PlanInstall.Add(installationEntry);

            return installationEntry;

        }

        private string _installationTempPath;
        private DirectoryInfo _installationTempDir;
        private static Random rnd = new Random();
        private IInstallRegistryProvider _installsRegistry;
        private IGithubApiRequestManager _githubApi;
        private IKoinoniaLogger _logger;

        private void AllocateTmp()
        {
            var userTmpPath = Path.GetTempPath();
            _installationTempPath = Path.Combine(userTmpPath, "KoinoniaInstallation" + rnd.Next());
            _installationTempDir = new DirectoryInfo(_installationTempPath);
            if(!_installationTempDir.Exists) _installationTempDir.Create();

            _logger.Log("Allocated temporary folder: "+_installationTempDir.FullName);

        }

        private void ReleaseTmp()
        {
            FileUtils.DeleteFolder(_installationTempDir);
            _logger.Log("Temporary Folder Released: " + _installationTempDir.FullName);

        }

        public InstallationResult Install(Downloadable downloadable)
        {
            try
            {
                Install_Internal(downloadable);
            }
            catch (ConfigDataNotFoundException ex)
            {
                _logger.LogProblem(ex.Message);
                return new InstallationResult()
                {
                    Success = false
                };
            }
            return new InstallationResult()
            {
                Success = true
            };
        }


        private void Install_Internal(Downloadable downloadable)
        {

            Add(downloadable);

            AllocateTmp();
            foreach (var entry in PlanInstall.ToArray())
            {
                AnalyzePackage(entry);
            }

            foreach (var entry in PlanInstall.ToArray())
            {
                ExtractPackage(entry);
            }

            foreach (var entry in PlanInstall.ToArray())
            {
                Commit(entry);
            }

            foreach (var entry in PlanInstall.ToArray())
            {
                RegisterInstall(entry);
            }

            _installsRegistry.Commit();

            foreach (var entry in PlanInstall)
            {
                _logger.Log("Installed " + entry.RepositoryName + " @ " + entry.Name);
            }

            ReleaseTmp();
        }

        private void RegisterInstall(InstallPlanEntry _)
        {
            _installsRegistry.AddInstall(new Install()
            {
                AuthorName = _.AuthorName,
                RepositoryName = _.RepositoryName,
                Name =  _.Name,
                CommitSha = _.CommitSha,
                Type = _.Type,
                ConfigData = _.ConfigData,
                Mappings = _.FullMappings
            });
        }

        private void AnalyzePackage(InstallPlanEntry entry)
        {

            _logger.Log("Checking dependencies in " + entry);

            if (entry.ConfigData.Dependencies.Any())
            {

                foreach (var dependency in entry.ConfigData.Dependencies)
                {
                    _logger.Log("   Processing " + dependency.Key+" @ "+dependency.Value);

                    var depData = GithubSchemeDecoder.DecodeShort(dependency.Key);

                    // See if this package is already planned to be installed
                    var depEntry =
                        PlanInstall.FirstOrDefault(_ => depData.Owner == _.AuthorName && depData.Name == _.RepositoryName);


                    if (depEntry != null) continue; //Already been processed

                    // See if we got this package registered
                    var host =
                        _hostsRegistry.DownloadablesHosts.FirstOrDefault(_ => depData.Owner == _.AuthorName && depData.Name == _.RepositoryName);

                    // If not, create it locally
                    if (host == null)
                    {
                        host = new DownloadablesHost()
                        {
                            AuthorName = depData.Owner,
                            RepositoryName = depData.Name,
                        };
                    }

                    host.FetchDownloadables();

                    var downloadable = host.Downloadables.FirstOrDefault();

                    //TODO: need version here

                    if (!string.IsNullOrEmpty(dependency.Value))
                    {
                        downloadable = host.Downloadables.FirstOrDefault(_ => _.Name == dependency.Value); 
                    }

                    if (downloadable == null) throw new Exception("No Downloadables found for " + host);

                    var newEntry = Add(downloadable);

                    AnalyzePackage(newEntry);
                }
            }
            else
            {
                _logger.Log("   Seeing no dependencies for " + entry);
            }


        }

        private void Commit(InstallPlanEntry entry)
        {
     
            //DANCE, BABY, DANCE!

            var mappings = entry.ConfigData.Mappings;
            if (mappings != null)
            {

                _logger.Log("Mappings found...");

                if (!string.IsNullOrEmpty(mappings.Default))
                {
                    _logger.Log("Using default mapping: "+mappings.Default);
                    var rootPath = entry.TmpDir.GetDirectories().First().FullName;
                    rootPath = Path.Combine(rootPath, mappings.Default);
                    var rootDir = new DirectoryInfo(rootPath);
                    var destPath = Path.Combine(PathUtils.DefaultAbsolutePackagesPath, entry.RelativeDestPath);
                    var destDir = new DirectoryInfo(destPath);
                    _logger.Log("Copying from " + rootDir.FullName + " to " + destDir.FullName);
                    FileUtils.CopyFilesRecursively(rootDir, destDir);
                    entry.FullMappings.Default = Path.Combine(PathUtils.DefaultRelativePackagesPath,entry.RelativeDestPath);
                }

                if (!string.IsNullOrEmpty(mappings.Root))
                {
                    _logger.Log("Using root mapping: "+mappings.Root);
                    var rootPath = entry.TmpDir.GetDirectories().First().FullName;
                    rootPath = Path.Combine(rootPath, mappings.Root);
                    var rootDir = new DirectoryInfo(rootPath);
                    var destPath = Path.Combine(PathUtils.RootPath, mappings.Root);
                    var destDir = new DirectoryInfo(destPath);
                    _logger.Log("Copying from " + rootDir.FullName + " to " + destDir.FullName);
                    FileUtils.CopyFilesRecursively(rootDir, destDir);
                    entry.FullMappings.Root = Path.Combine(PathUtils.RootPath, mappings.Root);
                }

            }
            else
            {
                var rootPath = entry.TmpDir.GetDirectories().First().FullName;
                var rootDir = new DirectoryInfo(rootPath);
                var destPath = Path.Combine(PathUtils.DefaultAbsolutePackagesPath, entry.RelativeDestPath);
                var destDir = new DirectoryInfo(destPath);

                _logger.Log("Mappings not found. Using standard paths. ");
                _logger.Log("Copying from " + rootDir.FullName + " to " + destDir.FullName);
                FileUtils.CopyFilesRecursively(rootDir, destDir);

                entry.FullMappings.Default = Path.Combine(PathUtils.DefaultRelativePackagesPath, entry.RelativeDestPath); 
            }

            _logger.Log("Saving config data... ");

            var kcPath = Path.Combine(entry.TmpDir.GetDirectories().First().FullName, "koinonia.config.json");
            var kcDestPath = Path.Combine(PathUtils.DefaultAbsolutePackagesPath, entry.RelativeDestPath);
            var cFilePath = Path.Combine(kcDestPath, "koinonia.config.json");
            if (!File.Exists(cFilePath))
            {
                entry.FullMappings.Default = Path.Combine(PathUtils.DefaultRelativePackagesPath, entry.RelativeDestPath); 
                File.Copy(kcPath, cFilePath);
            }

            kcPath = Path.Combine(entry.TmpDir.GetDirectories().First().FullName, "installer.kcs");

            if (File.Exists(kcPath))
            {
                _logger.Log("Saving installer... ");

                kcDestPath = Path.Combine(PathUtils.DefaultAbsolutePackagesPath, entry.RelativeDestPath);
                cFilePath = Path.Combine(kcDestPath, "installer.kcs");
                if (!File.Exists(cFilePath))
                {
                    File.Copy(kcPath, cFilePath);
                }
            }

            


        }

        private void ExtractPackage(InstallPlanEntry entry)
        {
            entry.RelativeInstallationPath = entry.RepositoryName;
            entry.TmpPath = Path.Combine(_installationTempPath, entry.RelativeInstallationPath); 
            entry.TmpDir = new DirectoryInfo(entry.TmpPath); 
           
            //Download 

            _logger.Log("Downloading "+entry);
            var bytes = _githubApi.GetZipball(entry.AuthorName,entry.RepositoryName,entry.Name);


            _logger.Log("Decompressing " + entry);
            //Decompress
            ZipUtils.Decompress(entry.TmpPath, bytes);

        }

        internal class InstallPlanEntry
        {
            private Mappings _fullMappings;
            public string RelativeInstallationPath { get; set; }
            public DirectoryInfo TmpDir { get; set; }
            public string TmpPath { get; set; }
            public string Name { get; set; }
            public string CommitSha { get; set; }
            public string AuthorName { get; set; }
            public string RepositoryName { get; set; }
            public DownloadableType Type { get; set; }
            public ConfigData ConfigData { get; set; }

            public Mappings FullMappings
            {
                get { return _fullMappings ?? (_fullMappings = new Mappings()); }
                set { _fullMappings = value; }
            }

            public override string ToString()
            {
                return string.Format("{0}/{1}@{2} ({3})",AuthorName,RepositoryName,Name,Type); 
            }

            public string RelativeDestPath 
            {
                get
                {
                    var p = string.Format("{0}/{1}",AuthorName,RepositoryName);
                    return p;
                }
            }
        }
    }

    public class InstallationEntry
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string CommitSha { get; set; }
        [JsonProperty]
        public string AuthorName { get; set; }
        [JsonProperty]
        public string RepositoryName { get; set; }
        [JsonProperty]
        public DownloadableType Type { get; set; }
        [JsonProperty]
        public ConfigData ConfigData { get; set; }
        [JsonProperty]
        public Mappings Mappings { get; set; }
    }

    
}