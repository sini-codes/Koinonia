using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Koinonia
{

    public class ConfigData 
    {
        private Dictionary<string, string> _dependencies;

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Version { get; set; }

        [JsonProperty]
        public string License { get; set; }

        [JsonProperty]
        public Mappings Mappings { get; set; }

        [JsonProperty]
        public bool RequiresFullReimport { get; set; }

        [JsonProperty]
        public Dictionary<string, string> Dependencies
        {
            get { return _dependencies ?? (_dependencies = new Dictionary<string, string>()); }
            set { _dependencies = value; }
        }

        public static ConfigData FromJsonString(string json)
        {
            return JsonUtils.DeserializeObject<ConfigData>(json);
        }

    }


    

}
