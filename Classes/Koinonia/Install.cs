using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Koinonia
{

    public class Install
    {
        private Mappings _fullMappings;

        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string CommitSha { get; set; }
        [JsonProperty]
        public string AuthorName { get; set; }
        [JsonProperty]
        public string RepositoryName { get; set; }
        [JsonProperty]
        public DownloadableType Type { get; set; }
        [JsonProperty]
        public DateTime AssociatedDate { get; set; }
        [JsonProperty]
        public ConfigData ConfigData { get; set; }
        [JsonProperty]
        public Mappings Mappings { get; set; }
        [JsonProperty]
        public bool InstallFinalized { get; set; }

        public Mappings FullMappings
        {
            get { return _fullMappings ?? (_fullMappings = new Mappings()
            {
                Default = Path.Combine(PathUtils.RootPath, Mappings.Default),
                Root = string.IsNullOrEmpty(Mappings.Root) ? null : Path.Combine(PathUtils.RootPath, Mappings.Root),  

            }); }
            set { _fullMappings = value; }
        }

        public bool IsInstallOf(DownloadablesHost host)
        {
            return host.AuthorName == AuthorName && host.RepositoryName == RepositoryName;
        }

        public bool IsInstallOf(Downloadable host)
        {
            return host.AuthorName == AuthorName && host.RepositoryName == RepositoryName && host.Name == Name;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}@{2}#{3}",AuthorName,RepositoryName,Name,CommitSha);
        }

        public object ToShortString()
        {
            return string.Format("{0}/{1}",AuthorName, RepositoryName);
        }
    }

    public static class InstallExtensions
    {
        public static string GetInstallerPath(this Install install)
        {
            return Path.Combine(install.FullMappings.Default, "installer.kcs");
        }
    }

}


