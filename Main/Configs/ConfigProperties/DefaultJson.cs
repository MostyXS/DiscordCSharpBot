using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Valera
{
    struct DefaultJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("guildid")]
        public ulong GuildId { get; private set; }
        

    }
}
