using System;
using System.Collections.Generic;
using System.IO;

namespace Koinonia
{
    public class PackageConfigManager : IPackageConfigManager
    {
        private PackageConfig _packageConfig;
        private List<GithubRepositoryEntry> _dependenciesEntries;

        private IPackageConfigRepository _packageConfigRepository { get; set; }

        public PackageConfigManager(IPackageConfigRepository packageConfigRepository)
        {
            _packageConfigRepository = packageConfigRepository;
        }

        private PackageConfig PackageConfig
        {
            get
            {
                if (!IsAvailable) return new PackageConfig();
                return _packageConfig ?? (_packageConfig = _packageConfigRepository.Get());
            }
            set { _packageConfig = value; }
        }

        public void AddDependency(GithubRepositoryEntry dep)
        {
            DependenciesEntries.Add(dep);
        }

        public void RemoveDependency(GithubRepositoryEntry dep)
        {
            DependenciesEntries.Remove(dep);
        }

        private List<GithubRepositoryEntry> DependenciesEntries
        {
            get
            {
                if (_dependenciesEntries == null)
                {
                    _dependenciesEntries = new List<GithubRepositoryEntry>();

                    foreach (var dep in PackageConfig.Dependencies)
                    {
                        var entry = GithubSchemeDecoder.DecodeShort(dep.Key);
                        entry.Tag = dep.Value;
                    }

                }

                return _dependenciesEntries;
            }
            set { _dependenciesEntries = value; }
        }

        public IEnumerable<GithubRepositoryEntry> Dependencies
        {
            get
            {
                return DependenciesEntries;
            }
        }

        public bool IsAvailable
        {
            get { return _packageConfigRepository.IsAvailable; }
        }

        public string Name
        {
            get { return PackageConfig.Name; }
            set { PackageConfig.Name = value; }
        }

        public string Version
        {
            get { return PackageConfig.Version; }
            set { PackageConfig.Version = value; }
        }

        public string License
        {
            get { return PackageConfig.License; }
            set { PackageConfig.License = value; }
        }

        public void Save()
        {
            PackageConfig.Dependencies.Clear();
            foreach (var entry in DependenciesEntries)
            {
                PackageConfig.Dependencies[string.Format("{0}/{1}",entry.Owner,entry.Name)] = entry.Tag;
            }
            _packageConfigRepository.Commit(PackageConfig);
        }
    }

    public interface IPackageConfigRepository
    {
        PackageConfig Get();
        void Commit(PackageConfig config);
        bool IsAvailable { get; }
    }

    public class LocalPackageConfigRepository : IPackageConfigRepository
    {
        public LocalPackageConfigRepository(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }

        public PackageConfig Get()
        {
            if (!FileExists) return null;
            var json = File.ReadAllText(Path);
            return JsonUtils.DeserializeObject<PackageConfig>(json);
        }

        public void Commit(PackageConfig config)
        {
            if (!FileExists) File.Create(Path).Dispose();
            File.WriteAllText(Path, JsonUtils.SerializeObject(config).ToString());
        }

        public bool FileExists
        {
            get { return File.Exists(Path); }
        }

        public bool IsAvailable
        {
            get { return FileExists; }
        }
    }

    public class RemotePackageConfigRepository : IPackageConfigRepository
    {

        public RemotePackageConfigRepository(string url)
        {
            Url = url;
        }

        public string Url { get; private set; }

        private PackageConfig _cachedPackage { get; set; }

        public PackageConfig Get()
        {
            return _cachedPackage ??
                   JsonUtils.DeserializeObject<PackageConfig>(EditorUtils.GetText(Url, KoinoniaApplication.AccessToken));
        }

        public void Commit(PackageConfig config)
        {
            throw new NotImplementedException("Cannot commit to remote config.");
        }

        public bool IsAvailable
        {
            get { return true; }
        }
    }

}