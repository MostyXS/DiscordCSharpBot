using DSharpPlus;
using DSharpPlus.CommandsNext;
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
    public class StreamNotifier
    {
        DiscordGuild defaultGuild;
        public DiscordChannel NotifyChannel { private get; set; }
        List<ulong> currentlyStreaming = new List<ulong>();

        #region ConfigParams
        string defaultStartPhrase = string.Empty;
        string defaultEndPhrase = string.Empty;
        public ulong streamerRoleId;
        Dictionary<ulong, StreamerInfo> streamersInfo = new Dictionary<ulong, StreamerInfo>();
        #endregion

        public StreamNotifier(DiscordClient c, DiscordGuild defaultGuild, DiscordChannel streamNotificationChannel)
        {
            NotifyChannel = streamNotificationChannel;
            this.defaultGuild = defaultGuild;
            c.PresenceUpdated += Notify;
        }

        private async Task Notify(DiscordClient sender, PresenceUpdateEventArgs e)
        {
            if (NotifyChannel == null) return;
            var member = await defaultGuild.GetMemberAsync(e.User.Id);
            if (member == null || !member.Roles.Any((x) => x.Id == streamerRoleId)) return;

            var presAfter = e.UserAfter.Presence;
            var actTypeAfter = presAfter.Activity.ActivityType;
            string content;

            StreamerInfo info;
            bool isInList = streamersInfo.TryGetValue(member.Id, out info);

            if (actTypeAfter == ActivityType.Streaming && !currentlyStreaming.Contains(member.Id)) //If start streaming
            {
                currentlyStreaming.Add(member.Id);
                content = isInList && !string.IsNullOrEmpty(info.StartPhrase) ? info.StartPhrase : defaultStartPhrase;

                var actInfo = presAfter.Activity.RichPresence;
                content.Replace("name", member.Username);
                content.Replace("game", actInfo.Application.Name);

                var embedBuilder = new DiscordEmbedBuilder();
                embedBuilder.SetAuthor(member);
                embedBuilder.SetTitle(actInfo.Details);
                embedBuilder.AddField("Игра", actInfo.State);

                if (actInfo.Application != null && actInfo.Application.CoverImageUrl != null)
                    embedBuilder.ImageUrl = actInfo.Application.CoverImageUrl.ToString();
                embedBuilder.AddField("Ссылка", presAfter.Activity.StreamUrl);

                await NotifyChannel.SendMessageAsync(content, embed: embedBuilder);

            }
            else if (actTypeAfter != ActivityType.Streaming && currentlyStreaming.Contains(member.Id)) // If end streaming
            {
                currentlyStreaming.Remove(member.Id);
                content = isInList && !string.IsNullOrEmpty(info.EndPhrase) ? info.EndPhrase : defaultEndPhrase;

                content.Replace("name", member.Username);
                await NotifyChannel.SendMessageAsync(content);
            }
        }

        #region Commands
        public async Task SetStreamerRole(DiscordRole role)
        {
            streamerRoleId = role.Id;
            await SaveAsync();
        }
        
        public async Task SetStartPhrase(bool isDefault, string p, CommandContext ctx = null)
        {
            if (isDefault)
            {
                await ctx.Channel.SendTempMessageAsync($"Успешно установлена как стандартная фраза для начала стрима: \"{p}\"");
                defaultStartPhrase = p;
            }
            else 
            {
                var member = ctx.Member;
                if (member.Roles.Any((x) => x.Id == streamerRoleId))
                {
                    var memberId = member.Id;
                    if (!streamersInfo.ContainsKey(member.Id))
                    {
                        streamersInfo.Add(memberId, new StreamerInfo());
                    }
                    var info = streamersInfo[memberId];
                    info.StartPhrase = p;
                    streamersInfo[memberId] = info;
                    await ctx.Channel.SendTempMessageAsync($"Успешно установлена фраза для начала стрима: \"{p}\" у пользователя {ctx.Member.Mention}");
                }
            }
            await SaveAsync();
        }

        public async Task SetEndPhrase(bool isDefault, string p, CommandContext ctx = null)
        {
            var member = ctx?.Member;
            if (isDefault)
            {
                await ctx.Channel.SendTempMessageAsync($"Успешно установлена как стандартная фраза для окончания стрима: \"{p}\"");
                defaultEndPhrase = p;
            }
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
                await ctx.Channel.SendTempMessageAsync($"Успешно установлена фраза для окончания стрима: \"{p}\" у пользователя {ctx.Member.Mention}");
            }
            await SaveAsync();
        }
        #endregion

        #region Config Management
        private async Task SaveAsync()
        {
            StreamNotifierJson json = new StreamNotifierJson();
            json.DefaultStartPhrase = defaultStartPhrase;
            json.DefaultEndPhrase = defaultEndPhrase;
            json.StreamerRoleId = streamerRoleId;
            json.StreamersInfo = streamersInfo;
            var jsonString = JsonConvert.SerializeObject(json);
            await File.WriteAllTextAsync(ConfigNames.STREAMNOTY, jsonString);
        }

        public async Task TryInitializeAsync()
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(ConfigNames.STREAMNOTY);
                var json = JsonConvert.DeserializeObject<StreamNotifierJson>(jsonString);

                streamerRoleId = json.StreamerRoleId;
                streamersInfo = json.StreamersInfo;
                if (json.DefaultStartPhrase != null)
                    defaultStartPhrase = json.DefaultStartPhrase;
                if (json.DefaultEndPhrase != null)
                    defaultEndPhrase = json.DefaultEndPhrase;
            }
            catch
            {

            }
        }
        #endregion

        #region InnerStructs
        public struct StreamerInfo
        {
            public string StartPhrase { get; set; }
            public string EndPhrase { get; set; }
        }
        struct StreamNotifierJson
        {
            public string DefaultStartPhrase { get; set; }
            public string DefaultEndPhrase { get; set; }
            public ulong StreamerRoleId { get; set; }
            public Dictionary<ulong, StreamerInfo> StreamersInfo { get; set; }
            
        }
        #endregion

    }

}
