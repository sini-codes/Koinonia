using System;
using System.Collections.Generic;
using System.Linq;

namespace Koinonia
{
    public class GithubApi : IGithubApi
    {

        private IWebRequestManager _githubApiRequestManager;
        private IWebRequestManager _commonWebRequestManager;

        public GithubApi(IWebRequestManager githubApiRequestManager, IWebRequestManager commonWebRequestManager)
        {
            _githubApiRequestManager = githubApiRequestManager;
            _commonWebRequestManager = commonWebRequestManager;
        }

        public List<GithubTag> GetTags(string authorName, string repositoryName)
        {

            var jsonString = _githubApiRequestManager.GetText(GetTagsUrl(authorName, repositoryName));
            var jsonObject = JSON.Parse(jsonString).AsArray;

            return jsonObject.Childs.Select(child => new GithubTag()
            {
                CommitSha = child["commit"]["sha"].AsString,
                Name = child["name"].AsString
            }).ToList();

        }

        public List<GithubBranch> GetBranches(string authorName, string repositoryName)
        {
            var jsonString = _githubApiRequestManager.GetText(GetBranchesUrl(authorName, repositoryName));
            var jsonObject = JSON.Parse(jsonString).AsArray;

            return jsonObject.Childs.Select(child => new GithubBranch()
            {
                CommitSha = child["commit"]["sha"].AsString,
                Name = child["name"].AsString
            }).ToList();
        }

        public List<GithubRelease> GetReleases(string authorName, string repositoryName)
        {

            var jsonString = _githubApiRequestManager.GetText(GetReleasesUrl(authorName, repositoryName));
            var jsonObject = JSON.Parse(jsonString).AsArray;

            return jsonObject.Childs.Select(child =>
            {
                var dateTime = DateTime.Parse(child["published_at"].AsString);
                return new GithubRelease()
                {
                    Name = child["name"].AsString,
                    TagName = child["tag_name"].AsString,
                    Date = dateTime,
                };
            }).ToList();
        }



        public string GetConfigDataString(string authorName, string repositoryName, string commitSha)
        {
            return _githubApiRequestManager.GetText(GetConfigDataUrl(authorName, repositoryName, commitSha)); 
        }

        public IEnumerable<GithubRepositoriesRegistryEntry> GetGithubRepositoriesRegistry(string url)
        {

            var jsonString = _commonWebRequestManager.GetText(url);
            var json = JSON.Parse(jsonString).AsArray;


            foreach (var child in json.Childs)
            {

                var title = child["title"].AsString;
                var code = child["code"];

                var s = GithubSchemeDecoder.DecodeShort(code);

                yield return new GithubRepositoriesRegistryEntry()
                {
                    Title = title,
                    RepositoryName = s.Name,
                    AuthorName = s.Owner
                };

            }

        }

        public byte[] GetZipball(string authorName, string repositoryName, string id)
        {
            return _githubApiRequestManager.GetBytes(GetZipballUrl(authorName, repositoryName, id));
        }

        public string GetReleasesUrl(string authorName, string repositoryName)
        {
            return string.Format("https://api.github.com/repos/{0}/{1}/releases", authorName, repositoryName);
        }

        public string GetTagsUrl(string authorName, string repositoryName)
        {
            return string.Format("https://api.github.com/repos/{0}/{1}/tags",authorName,repositoryName);
        }

        public string GetZipballUrl(string authorName, string repositoryName, string id)
        {
            return string.Format("https://api.github.com/repos/{0}/{1}/zipball/{2}", authorName, repositoryName, id);
        }

        public string GetBranchesUrl(string authorName, string repositoryName)
        {
            return string.Format("https://api.github.com/repos/{0}/{1}/branches",authorName,repositoryName);
        }

        public string GetConfigDataUrl(string authorName, string repositoryName, string sha)
        {
            return string.Format("https://raw.githubusercontent.com/{0}/{1}/{2}/koinonia.config.json", authorName, repositoryName, sha);
        }

    }
}