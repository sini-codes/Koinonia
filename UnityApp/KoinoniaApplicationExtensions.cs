using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Koinonia
{
    public static class KoinoniaApplicationExtensions
    {

        public static IGithubApi GithubApiManager
        {
            get { return KoinoniaApplication.Instance.GithubApi; }
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