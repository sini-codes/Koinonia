using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Koinonia
{


    public static class TSAssetDatabase
    {
        public static void Refresh()
        {
            ThreadingUtils.WaitOnMainThread(AssetDatabase.Refresh);
        }
    }

    public static class TSApplication
    {
        public static string DataPath
        {
            get { return ThreadingUtils.GetOnMainThread(() => Application.dataPath); }
        }
    }

    public class KoinoniaWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = 1000*30;
            return webRequest;
        }
    }

    public static class EditorUtils
    {


        private static WebClient GetWebClient(string accessToken)
        {
            var wc = new KoinoniaWebClient();

            if (accessToken != null)
            {
                wc.Headers.Add("Authorization", "token " + accessToken);
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            }

            return wc;
        }

        public static byte[] GetBytes(string url, string accessToken = null)
        {
            using (var wc = GetWebClient(accessToken))
            {
                return wc.DownloadData(url);
            }
        }

        public static string GetText(string url, string accessToken = null)
        {
            using (var wc = GetWebClient(accessToken))
            {
                return wc.DownloadString(url);
            }
        }

        public static T GetSerializedObject<T>(string url, string accessToken = null)
        {
            return JsonUtils.DeserializeObject<T>(GetText(url, accessToken));
        }

    }

    public interface IGithubApiRequestManager
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

    public class GithubTag
    {
        public string Name { get; set; }
        public string CommitSha { get; set; }
    }

    public class GithubBranch
    {
        public string Name { get; set; }
        public string CommitSha { get; set; }
    }

    public class GithubRelease
    {
        public string Name { get; set; }
        public string TagName { get; set; }
        public DateTime Date { get; set; }
        public string CommitSha { get; set; }
    }

    public class GithubRepositoriesRegistryEntry
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string RepositoryName { get; set; }
    }


    public class UnityGithubApiRequestManager : IGithubApiRequestManager
    {
        public string AccessToken { get; set; }

        public UnityGithubApiRequestManager(string accessToken)
        {
            AccessToken = accessToken;
        }

        public List<GithubTag> GetTags(string authorName, string repositoryName)
        {

            var jsonString = EditorUtils.GetText(GetTagsUrl(authorName, repositoryName), AccessToken);
            var jsonObject = JSON.Parse(jsonString).AsArray;

            return jsonObject.Childs.Select(child => new GithubTag()
            {
                CommitSha = child["commit"]["sha"].AsString,
                Name = child["name"].AsString
            }).ToList();

        }

        public List<GithubBranch> GetBranches(string authorName, string repositoryName)
        {
            var jsonString = EditorUtils.GetText(GetBranchesUrl(authorName, repositoryName), AccessToken);
            var jsonObject = JSON.Parse(jsonString).AsArray;

            return jsonObject.Childs.Select(child => new GithubBranch()
            {
                CommitSha = child["commit"]["sha"].AsString,
                Name = child["name"].AsString
            }).ToList();
        }

        public List<GithubRelease> GetReleases(string authorName, string repositoryName)
        {

            var jsonString = EditorUtils.GetText(GetReleasesUrl(authorName, repositoryName), AccessToken);
            var jsonObject = JSON.Parse(jsonString).AsArray;

            return jsonObject.Childs.Select(child => new GithubRelease()
            {
                Name = child["name"].AsString,
                TagName = child["tag_name"].AsString,
                Date = DateTime.Parse(child["published_at"].AsString),
            }).ToList();
        }



        public string GetConfigDataString(string authorName, string repositoryName, string commitSha)
        {
            return EditorUtils.GetText(GetConfigDataUrl(authorName, repositoryName, commitSha)); 
        }

        public IEnumerable<GithubRepositoriesRegistryEntry> GetGithubRepositoriesRegistry(string url)
        {

            var jsonString = EditorUtils.GetText(url);
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
            return EditorUtils.GetBytes(GetZipballUrl(authorName, repositoryName, id), AccessToken);
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
            return string.Format("http://github-raw-cors-proxy.herokuapp.com/{0}/{1}/blob/{2}/koinonia.config.json", authorName, repositoryName, sha);
        }

    }


}

