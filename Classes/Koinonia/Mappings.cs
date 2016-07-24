namespace Koinonia
{
    public class Mappings
    {
        [JsonProperty]
        public string Default { get; set; }
        [JsonProperty]
        public string Root { get; set; }
        [JsonProperty]
        public string Docs { get; set; }
        [JsonProperty]
        public string Tests { get; set; }
    }
}