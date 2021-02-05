using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSSKeeper.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LSSKeeper.Notifications
{
    public class StreamNotifier
    {
        DiscordGuild defaultGuild;
        public DiscordChannel NotifyChannel { private get; set; }
        List<ulong> currentlyStreaming = new List<ulong>();

        #region ConfigParams
        string defaultStartPhrase = string.Empty;
        string defaultImageUrl = string.Empty;
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

            var pres = e.UserAfter.Presence;
            var actTypeAfter = pres.Activity.ActivityType;
            string content;

            StreamerInfo info;
            bool isInList = streamersInfo.TryGetValue(member.Id, out info);

            if (actTypeAfter == ActivityType.Streaming && !currentlyStreaming.Contains(member.Id)) //If start streaming
            {
                currentlyStreaming.Add(member.Id);
                content = isInList && !string.IsNullOrEmpty(info.StartPhrase) ? info.StartPhrase : defaultStartPhrase;
                var imageUrl = isInList && !string.IsNullOrEmpty(info.ImageUrl) ? info.ImageUrl : defaultImageUrl;

                var actInfo = pres.Activity.RichPresence;
                content = content.Replace("name", $"**{member.Username}**", StringComparison.OrdinalIgnoreCase);
                content = content.Replace("game", $"**{actInfo.State}**", StringComparison.OrdinalIgnoreCase);

                var embedBuilder = new DiscordEmbedBuilder();
                embedBuilder.SetAuthor(member);
                embedBuilder.SetTitle(actInfo.Details);
                embedBuilder.AddField("Игра", actInfo.State);
                embedBuilder.AddField("Ссылка", pres.Activity.StreamUrl);
                embedBuilder.WithImageUrl(imageUrl);

                await NotifyChannel.SendMessageAsync(content, embed: embedBuilder.Build());

            }
            else if (actTypeAfter != ActivityType.Streaming && currentlyStreaming.Contains(member.Id)) // If end streaming
            {
                currentlyStreaming.Remove(member.Id);
                content = isInList && !string.IsNullOrEmpty(info.EndPhrase) ? info.EndPhrase : defaultEndPhrase;

                content = content.Replace("name", $"**{member.Username}**");
                await NotifyChannel.SendMessageAsync(content);
            }
        }

        #region Commands
        public async Task SetStreamerRole(DiscordRole role)
        {
            streamerRoleId = role.Id;
            await SaveAsync();
        }

        public async Task SetStreamerInfo(string content, StreamerInfoType infoType, CommandContext ctx)
        {

            var member = ctx.Member;
            if (!member.Roles.Any((x) => x.Id == streamerRoleId)) return;

            var memberId = member.Id;
            if (!streamersInfo.ContainsKey(memberId))
            {
                streamersInfo.Add(memberId, new StreamerInfo());
            }
            string actionType = string.Empty;
            var info = streamersInfo[memberId];
            switch (infoType)
            {
                case StreamerInfoType.StartPhrase:
                    {
                        actionType = "фраза для начала стрима";
                        info.StartPhrase = content;
                        break;
                    }
                case StreamerInfoType.ImageUrl:
                    {
                        actionType = "картинка для стрима";
                        info.ImageUrl = content;
                        break;
                    }
                case StreamerInfoType.EndPhrase:
                    {
                        actionType = "фраза для окончания стрима";
                        info.EndPhrase = content;
                        break;
                    }
            }
            streamersInfo[memberId] = info;
            await SaveAsync();

            await ctx.Channel.SendTempMessageAsync($"Успешно установлена {actionType}: \"{content}\" у пользователя {ctx.Member.Mention}");

        }
        /// <summary>
        /// returns result of operation
        /// </summary>
        /// <param name="content"></param>
        /// <param name="infoType"></param>
        /// <returns></returns>
        public async Task<string> SetDefaultStreamerInfo(string content, StreamerInfoType infoType)
        {
            string actionType = string.Empty;
            switch (infoType)
            {
                case StreamerInfoType.StartPhrase:
                    {
                        actionType = "фраза для начала стрима";
                        defaultStartPhrase = content;
                        break;
                    }
                case StreamerInfoType.ImageUrl:
                    {
                        actionType = "картинка для стрима";
                        defaultImageUrl = content;
                        break;
                    }
                case StreamerInfoType.EndPhrase:
                    {
                        actionType = "фраза для окончания стрима";
                        defaultImageUrl = content;
                        break;
                    }
            }
            await SaveAsync();
            return $"Стандартная {actionType} успешно установлена";
        }
        #endregion

        #region Config Management
        private async Task SaveAsync()
        {
            StreamNotifierJson json = new StreamNotifierJson();
            json.DefaultStartPhrase = defaultStartPhrase;
            json.DefaultImageUrl = defaultImageUrl;
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
            public string ImageUrl { get; set; }
            public string StartPhrase { get; set; }
            public string EndPhrase { get; set; }
        }
        struct StreamNotifierJson
        {
            public string DefaultStartPhrase { get; set; }
            public string DefaultImageUrl { get; set; }
            public string DefaultEndPhrase { get; set; }
            public ulong StreamerRoleId { get; set; }
            public Dictionary<ulong, StreamerInfo> StreamersInfo { get; set; }
            
        }
        #endregion
    }
}
