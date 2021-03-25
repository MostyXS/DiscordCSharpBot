using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;

namespace Volodya.VKNotifying
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    class VKNotifier
    {
        [JsonIgnore]
        DiscordChannel vkNotyChannel;


        #region ConfigParams
        



        #endregion

        private async Task RunAsync()
        {
            await TryInitializeAsync();
            VkApi api = new VkApi();
            api.Authorize(GetVkConfigParams());

            //var server = await api.Groups.GetLongPollServerAsync();

            while(true)
            {



                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        private async Task TryInitializeAsync()
        {
            throw new NotImplementedException();
        }

        private ApiAuthParams GetVkConfigParams()
        {
            return new ApiAuthParams()
            {
                //AccessToken = appToken
            };
        }

        struct VKJson
        {
            
            string guildId;
            string notificationChannelId;


        }


    }
}
