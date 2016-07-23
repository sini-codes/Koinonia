using System.Collections.Generic;

namespace Koinonia
{
    public interface IGithubApi
    {
        List<GithubTag> GetTags(string authorName, string repositoryName);
        List<GithubBranch> GetBranches(string authorName, string repositoryName);
        List<GithubRelease> GetReleases(string authorName, string repositoryName);
        string GetConfigDataString(string authorName, string repositoryName, string commitSha);
        IEnumerable<GithubRepositoriesRegistryEntry> GetGithubRepositoriesRegistry(string url);
        byte[] GetZipball(string authorName, string repositoryName, string id);
        string GetReleasesUrl(string authorName, string repositoryName);
        string GetTagsUrl(string authorName, string repositoryName);
        string GetZipballUrl(string authorName, string repositoryName, string id);
        string GetBranchesUrl(string authorName, string repositoryName);
        string GetConfigDataUrl(string authorName, string repositoryName, string sha);
    }
}