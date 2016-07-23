using System;
using System.Text.RegularExpressions;

namespace Koinonia
{
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
}