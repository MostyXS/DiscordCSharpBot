using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSSKeeper.Main.Configs.ConfigProperties
{
    struct VKJson
    {
        [JsonProperty("appToken")]
        public string AppToken { get; private set; }
        [JsonProperty("groupId")]
        public string GroupID { get; private set; }
    }
}
