using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LOSCKeeper
{
    struct RoleGranterJson
    {
        [JsonProperty("rolesMsgChannelId")]
        public ulong? RolesMessageChannelId { get; set; }

        [JsonProperty("roleMsgId")]
        public ulong? RolesMessageId { get; set; }

        [JsonProperty("rolesIdsFromEmojisIds")]
        public Dictionary<string, ulong> RolesIdsFromEmojisIds { get; set; }


    }
}
