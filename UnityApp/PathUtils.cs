using System.IO;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Koinonia
{
    public static class PathUtils
    {
        private static string _rootPath;
        private static string _tempPath;
        private static string _defaultPackagesPath;
        private static string _packageConfigurationPath;
        private static string _repositoryCachePath;
        private static string _defaultRelativePackagesPath;
        private static string _installRegistryPath;
        private static string _testRelativePackagesPath;
        private static string _testsAbsolutePackagesPath;
        private static string _docsRelativePackagesPath;
        private static string _docsAbsolutePackagesPath;

        public static string RootPath
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

        public static string InstallRegistryPath
        {
            get { return _installRegistryPath ?? (_installRegistryPath = Path.Combine(RootPath,".koinonia/install_registry.json")); }
            set { _installRegistryPath = value; }
        }

        public static string DefaultAbsolutePackagesPath
        {
            get { return _defaultPackagesPath ?? (_defaultPackagesPath = Path.Combine(RootPath, DefaultRelativePackagesPath)); }
            private set { _defaultPackagesPath = value; }
        }

        public static string TestsAbsolutePackagesPath
        {
            get { return _testsAbsolutePackagesPath ?? (_testsAbsolutePackagesPath = Path.Combine(RootPath, TestRelativePackagesPath)); }
            private set { _testsAbsolutePackagesPath = value; }
        }

        public static string DocsAbsolutePackagesPath
        {
            get { return _docsAbsolutePackagesPath ?? (_docsAbsolutePackagesPath = Path.Combine(RootPath, DocsRelativePackagesPath)); }
            private set { _docsAbsolutePackagesPath = value; }
        }

        public static string DefaultRelativePackagesPath
        {
            get { return _defaultRelativePackagesPath ?? (_defaultRelativePackagesPath =  "Assets/Plugins/ManagedPackages"); }
            private set { _defaultRelativePackagesPath = value; }
        }

        public static string TestRelativePackagesPath
        {
            get { return _testRelativePackagesPath ?? (_testRelativePackagesPath =  "Assets/Tests"); }
            private set { _testRelativePackagesPath = value; }
        }

        public static string DocsRelativePackagesPath
        {
            get { return _docsRelativePackagesPath ?? (_docsRelativePackagesPath =  "Documentation"); }
            private set { _docsRelativePackagesPath = value; }
        }

        public static string PackageConfigurationPath
        {
            get { return _packageConfigurationPath ?? (_packageConfigurationPath = Path.Combine(RootPath, ".koinonia/package.json")); }
            private set { _packageConfigurationPath = value; }
        }

        public static string RepositoryCachePath
        {
            get { return _repositoryCachePath ?? (_repositoryCachePath = Path.Combine(RootPath, ".koinonia/repository_cache.json")); }
            private set { _repositoryCachePath = value; }
        }

    }
}