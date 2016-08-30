using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Koinonia;


namespace Koinonia
{

    public class DownloadablesHost
    {
        private List<Downloadable> _downloadables;

        public string AuthorName { get; set; }
        public string RepositoryName { get; set; }

        //TagsUrl = string.Format("https://api.github.com/repos/{0}/{1}/tags", Owner, RepositoryName);
        //BranchesUrl = string.Format("https://api.github.com/repos/{0}/{1}/branches", Owner, RepositoryName);

        public List<Downloadable> Downloadables
        {
            get { return _downloadables ?? (_downloadables = new List<Downloadable>()); }
            set { _downloadables = value; }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", AuthorName, RepositoryName);
        }
    }

    public static class DownloadablesHostExtensions
    {

        public static IGithubApi GithubApiManager
        {
            get { return KoinoniaApplication.Instance.GithubApi; }
        }

        public static DownloadablesHost FetchInstall(this DownloadablesHost host, bool force = false)
        {
            return host;
        }

        public static DownloadablesHost FetchDownloadables(this DownloadablesHost host, bool force = false)
        {
            if (!force && host.Downloadables.Any()) return host;

            host.Downloadables.Clear();

            var githubTags = GithubApiManager
                .GetTags(host.AuthorName,host.RepositoryName).ToArray();

            var tags = githubTags
                .Select(_ => new Downloadable()
                {
                    AuthorName = host.AuthorName,
                    RepositoryName = host.RepositoryName,
                    CommitSha = _.CommitSha,
                    Name = _.Name,
                    Type = DownloadableType.Tag,
                    AssociatedDate = DateTime.Now
                }).ToArray();

            var branches = GithubApiManager
                .GetBranches(host.AuthorName, host.RepositoryName)
                .Select(_ => new Downloadable
                {
                    AuthorName = host.AuthorName,
                    RepositoryName = host.RepositoryName,
                    CommitSha = _.CommitSha,
                    Name = _.Name,
                    Type =  DownloadableType.Branch,
                    AssociatedDate = DateTime.Now
                }).ToArray();

            var releases = GithubApiManager
                .GetReleases(host.AuthorName, host.RepositoryName)
                .OrderByDescending(_ => _.Date)
                .Select(_ =>
                {
                    var tag = tags.FirstOrDefault(t=>t.Name == _.TagName);
                    return new Downloadable
                    {
                        AuthorName = host.AuthorName,
                        RepositoryName = host.RepositoryName,
                        CommitSha = tag.CommitSha,
                        Name = tag.Name,
                        Type = DownloadableType.Release,
                        AssociatedDate = _.Date
                    };
                }).ToArray();

            host.Downloadables.AddRange(releases);
            host.Downloadables.AddRange(tags);
            host.Downloadables.AddRange(branches);

            return host;
        }
    }


}


