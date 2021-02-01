using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LOSCKeeper.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Notifications
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StreamNotifier
    {
        public DiscordChannel NotifyChannel { private get; set; }

        DiscordGuild defaultGuild;
        [JsonProperty]
        string defaultStartPhrase;
        [JsonProperty]
        string defaultEndPhrase;
        [JsonProperty]
        Dictionary<ulong, StreamerInfo> streamersInfo = new Dictionary<ulong, StreamerInfo>();
        [JsonProperty]
        public ulong streamerRoleId;

        public StreamNotifier(DiscordGuild defaultGuild, DiscordChannel streamNotificationChannel, DiscordClient c)
        {
            NotifyChannel = streamNotificationChannel;
            this.defaultGuild = defaultGuild;
            c.PresenceUpdated += Notify;
        }

        public async Task SetStreamerRole(DiscordRole role)
        {
            streamerRoleId = role.Id;
            await Save();
        }
        public async Task SetStartPhrase(bool isDefault, string p, DiscordMember member = null)
        {
            if(isDefault) defaultStartPhrase = p;
            else if(member.Roles.Any((x) => x.Id == streamerRoleId))
            {
                var memberId = member.Id;
                if(!streamersInfo.ContainsKey(member.Id))
                {
                    streamersInfo.Add(memberId, new StreamerInfo());
                }
                var info = streamersInfo[memberId];
                info.StartPhrase = p;
                streamersInfo[memberId] = info;
            }
            await Save();
        }

        public async Task SetEndPhrase(bool isDefault, string p, DiscordMember member = null)
        {
            if (isDefault) defaultEndPhrase = p;
            else if (member.Roles.Any((x) => x.Id == streamerRoleId))
            {
                var memberId = member.Id;
                if (!streamersInfo.ContainsKey(member.Id))
                {
                    streamersInfo.Add(memberId, new StreamerInfo());
                }
                var info = streamersInfo[memberId];
                info.EndPhrase = p;
                streamersInfo[memberId] = info;
            }
            await Save();
        }

        private async Task Notify(DiscordClient sender, PresenceUpdateEventArgs e)
        {
            var member = await defaultGuild.GetMemberAsync(e.User.Id);
            if (member == null || !member.Roles.Any((x) => x.Id == streamerRoleId)) return;

            var presBefore = e.UserBefore.Presence;
            var presAfter = e.UserAfter.Presence;

            var actTypeBefore = presBefore.Activity.ActivityType;
            var actTypeAfter = presAfter.Activity.ActivityType;

            if (actTypeBefore == actTypeAfter) return;
            string content;

            StreamerInfo info;
            bool isInList = streamersInfo.TryGetValue(member.Id, out info);

            if (actTypeAfter == ActivityType.Streaming) //If start streaming
            {
                content = isInList && !string.IsNullOrEmpty(info.StartPhrase) ? info.StartPhrase : defaultStartPhrase;
                
                var actInfo = presAfter.Activity.RichPresence;
                content.Replace("name", member.Nickname);
                content.Replace("game", actInfo.Application.Name);

                var embedBuilder = new DiscordEmbedBuilder();
                embedBuilder.SetAuthor(member);
                embedBuilder.SetTitle(actInfo.Details);
                embedBuilder.AddField("Игра", actInfo.Application.Name);
                embedBuilder.ImageUrl = actInfo.LargeImage.Url.ToString();
                embedBuilder.AddField("Ссылка", presAfter.Activity.StreamUrl);

                await NotifyChannel.SendMessageAsync(content, embed: embedBuilder);

            }
            else if (actTypeBefore == ActivityType.Streaming) // If end streaming
            {
                content = isInList && !string.IsNullOrEmpty(info.EndPhrase) ? info.EndPhrase : defaultEndPhrase;

                content.Replace("name", member.Nickname);
                await NotifyChannel.SendMessageAsync(content);
            }
        }

        public async Task Save()
        {
            
            var jsonString = JsonConvert.SerializeObject(this);
            await File.WriteAllTextAsync(ConfigNames.STREAMNOTY, jsonString);
        }

        public async Task TryInitializeAsync()
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(ConfigNames.STREAMNOTY);
                var json = JsonConvert.DeserializeObject<StreamNotifier>(jsonString);
                streamerRoleId = json.streamerRoleId;
                streamersInfo = json.streamersInfo;
                defaultStartPhrase = json.defaultStartPhrase;
                defaultEndPhrase = json.defaultEndPhrase;
            }
            catch
            {

            }
        }

    }
    public struct StreamerInfo
    {
        public string StartPhrase { get; set; }
        public string EndPhrase { get; set; }
    }
}
