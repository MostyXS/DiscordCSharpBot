using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSSKeeper.Extensions;
using LSSKeeper.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LSSKeeper.Notifications
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class StreamNotifier
    {
        [JsonIgnore]
        List<ulong> currentlyStreaming = new List<ulong>();
        [JsonIgnore]
        DiscordChannel notifyChannel;
        [JsonIgnore]
        DiscordGuild defaultGuild;

        #region ConfigParams
        
        string defaultStartPhrase = "Empty";
        string defaultImageUrl = "Empty";
        string defaultEndPhrase = "Empty";
        ulong? notifyChannelId;
        ulong streamerRoleId;
        Dictionary<ulong, StreamerInfo> streamersInfo = new Dictionary<ulong, StreamerInfo>();
        #endregion

        private async Task Notify(DiscordClient sender, PresenceUpdateEventArgs e)
        {
            if (notifyChannel == null) return;
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

                try
                {
                    embedBuilder.WithImageUrl(imageUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось установить картинку для пользователя " + member.Username + " Причина: " + ex.Message);
                }
                
                
                await notifyChannel.SendMessageAsync(content, embed: embedBuilder.Build());

            }
            else if (actTypeAfter != ActivityType.Streaming && currentlyStreaming.Contains(member.Id)) // If end streaming
            {
                currentlyStreaming.Remove(member.Id);
                content = isInList && !string.IsNullOrEmpty(info.EndPhrase) ? info.EndPhrase : defaultEndPhrase;

                content = content.Replace("name", $"**{member.Username}**");
                await notifyChannel.SendMessageAsync(content);
            }
        }

        #region Commands
        public async Task SetStreamerRole(DiscordRole role)
        {
            
            streamerRoleId = role.Id;
            await SaveAsync();
        }

        public async Task SetChannelAsync(DiscordChannel channel)
        {
            notifyChannel = channel;
            notifyChannelId = channel.Id;
            await SaveAsync();
        }
        public async Task SetStreamerInfo(CommandContext ctx, string content, StreamerInfoType infoType)
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
                        defaultEndPhrase = content;
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
            try
            {
                var jsonString = JsonConvert.SerializeObject(this);
                await File.WriteAllTextAsync(ConfigNames.STREAMNOTY, jsonString);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("Не удалось сохранить конфиг оповещений о стриме " + e.Message);
            }
        }

        public async Task TryInitializeAsync(DiscordClient c, DiscordGuild defaultGuild)
        {
            this.defaultGuild = defaultGuild;
            try
            {
                var jsonString = await File.ReadAllTextAsync(ConfigNames.STREAMNOTY);
                var json = JsonConvert.DeserializeObject<StreamNotifier>(jsonString);
                defaultStartPhrase = json.defaultStartPhrase;
                defaultImageUrl = json.defaultImageUrl;
                defaultEndPhrase = json.defaultEndPhrase;
                if (json.notifyChannelId != null)
                {
                    notifyChannelId = json.notifyChannelId;
                    notifyChannel = defaultGuild.GetChannel((ulong)notifyChannelId);
                }
                streamerRoleId = json.streamerRoleId;
                streamersInfo = json.streamersInfo;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Не удалось инициализировать конфиг оповещений о стриме " + e.Message);

            }
            c.PresenceUpdated += Notify;
        }
        #endregion

        #region InnerStructs
        struct StreamerInfo
        {
            public string ImageUrl;
            public string StartPhrase;
            public string EndPhrase;
        }
        #endregion
    }
}
