using System.Collections.Generic;

namespace Koinonia
{
    public interface IPackageConfigManager
    {
        void AddDependency(GithubRepositoryEntry dep);
        void RemoveDependency(GithubRepositoryEntry dep);

        IEnumerable<GithubRepositoryEntry> Dependencies { get; }

        bool IsAvailable { get; }

        string Name { get; set; }
        string Version { get; set; }
        string License { get; set; }

        void Save();

    }


}