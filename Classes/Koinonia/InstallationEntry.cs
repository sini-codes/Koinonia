namespace Koinonia
{
    public class InstallationEntry
    {
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
        public ConfigData ConfigData { get; set; }
        [JsonProperty]
        public Mappings Mappings { get; set; }
    }
}