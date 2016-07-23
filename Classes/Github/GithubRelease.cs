using System;

namespace Koinonia
{
    public class GithubRelease
    {
        public string Name { get; set; }
        public string TagName { get; set; }
        public DateTime Date { get; set; }
        public string CommitSha { get; set; }
    }
}