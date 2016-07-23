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

        public static string DefaultRelativePackagesPath
        {
            get { return _defaultRelativePackagesPath ?? (_defaultRelativePackagesPath =  "Assets/Plugins/ManagedPackages"); }
            private set { _defaultRelativePackagesPath = value; }
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

    public interface IPathConfiguration
    {
        string RootPath { get; }
        string InstallRegistryPath { get; set; }
        string DefaultMappingPathAbsolute { get; }
        string DefaultMappingPathRelative { get; }
        string PackageConfigurationPath { get; }
        string RepositoryCachePath { get; }
    }
}