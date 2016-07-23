using System.IO;

namespace Koinonia
{
    public class UnityPathConfiguration : IPathConfiguration
    {
        private string _rootPath;
        private string _tempPath;
        private string _defaultPackagesPath;
        private string _packageConfigurationPath;
        private string _repositoryCachePath;
        private string _defaultRelativePackagesPath;
        private string _installRegistryPath;

        public string RootPath
        {
            get
            {
                if (_rootPath == null)
                {
                    _rootPath = TSApplication.DataPath;
                    _rootPath += "/../";
                }
                return _rootPath;
            }
            private set { _rootPath = value; }
        }

        public string InstallRegistryPath
        {
            get { return _installRegistryPath ?? (_installRegistryPath = Path.Combine(RootPath, ".koinonia/install_registry.json")); }
            set { _installRegistryPath = value; }
        }

        public string DefaultMappingPathAbsolute
        {
            get { return _defaultPackagesPath ?? (_defaultPackagesPath = Path.Combine(RootPath, DefaultMappingPathRelative)); }
            private set { _defaultPackagesPath = value; }
        }

        public string DefaultMappingPathRelative
        {
            get { return _defaultRelativePackagesPath ?? (_defaultRelativePackagesPath = "Assets/Plugins/ManagedPackages"); }
            private set { _defaultRelativePackagesPath = value; }
        }

        public string PackageConfigurationPath
        {
            get { return _packageConfigurationPath ?? (_packageConfigurationPath = Path.Combine(RootPath, ".koinonia/package.json")); }
            private set { _packageConfigurationPath = value; }
        }

        public string RepositoryCachePath
        {
            get { return _repositoryCachePath ?? (_repositoryCachePath = Path.Combine(RootPath, ".koinonia/repository_cache.json")); }
            private set { _repositoryCachePath = value; }
        }

    }
}