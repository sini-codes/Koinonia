using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace Koinonia
{

    public class PackageConfig
    {
        private Dictionary<string, string> _dependencies;

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Version { get; set; }

        [JsonProperty]
        public string License { get; set; }

        [JsonProperty]
        public Dictionary<string, string> Dependencies
        {
            get { return _dependencies ?? (_dependencies = new Dictionary<string, string>()); }
            set { _dependencies = value; }
        }
    }

    public class Mappings
    {
        [JsonProperty]
        public string Default { get; set; }
        [JsonProperty]
        public string Root { get; set; }
    }

    //Decomposes "owner/repo@tag" into separated entities
    public static class GithubSchemeDecoder
    {
        private static Regex regex = new Regex("(?<owner>.*)/(?<title>[^@#]*)(?<delimiter>[@#]?)(?<tag>.*)");
        private static Regex shortFormRegex = new Regex("(?<owner>.*)/(?<title>.*)");

        public static GithubRepositoryEntry Decode(string src)
        {
            Match match = regex.Match(src);
            if (match.Success)
            {
                return new GithubRepositoryEntry()
                {
                    Owner = match.Groups["owner"].Value,
                    Name = match.Groups["title"].Value,
                    Tag = match.Groups["tag"].Value
                };
            }
            else
            {
                throw new Exception("Failed to decode github scheme: " + src);
            }
        }

        public static GithubRepositoryEntry DecodeShort(string src)
        {
            Match match = shortFormRegex.Match(src);
            if (match.Success)
            {
                return new GithubRepositoryEntry()
                {
                    Owner = match.Groups["owner"].Value,
                    Name = match.Groups["title"].Value,
                };
            }
            else
            {
                throw new Exception("Failed to decode short github scheme: " + src);
            }
        }

   
    }

    public struct GithubRepositoryEntry
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Tag { get; set; }
    }

    public class GithubBranchDescriptor
    {
    }

    //GET https://api.github.com/repos/:owner/:repo/git/tags
    //Currently the same class is used to describe a branch but with fallback url:
    // https://github.com/:owner/:repo/archive/:tag.zip

  
    public class KoinoniaRepositoryConfiguration
    {
        private List<string> _dependencies;

        [JsonProperty]
        public List<string> dependencies
        {
            get { return _dependencies ?? (_dependencies = new List<string>()); }
            set { _dependencies = value; }
        }
    }
}