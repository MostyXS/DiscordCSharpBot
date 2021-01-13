using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LOSCKeeper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Notifications
{
    public class StreamNotifier
    {
        
        DiscordChannel notifyChannel;
        DiscordGuild defaultGuild;
        DiscordRole streamerRole;

        string streamStartPhrase;
        string streamEndPhrase;

        public StreamNotifier(DiscordGuild defaultGuild, DiscordChannel streamNotificationChannel, DiscordClient c)
        {
            notifyChannel = streamNotificationChannel;
            this.defaultGuild = defaultGuild;
            c.PresenceUpdated += Notify;
        }

        public void SetStreamerRole(DiscordRole role)
        {
            streamerRole = role;
        }
        public void SetWelcomePhrase(string p)
        {
            streamStartPhrase = p;
        }

        public void SetEndPhrase(string p)
        {
            streamEndPhrase = p;
        }


        private async Task Notify(DiscordClient sender, PresenceUpdateEventArgs e)
        {
            var member = await defaultGuild.GetMemberAsync(e.User.Id);
            if (member == null || !member.Roles.Any((x) => x.Id == streamerRole.Id)) return;
            var presBefore = e.UserBefore.Presence;
            var presAfter = e.UserAfter.Presence;
            var actTypeBefore = presBefore.Activity.ActivityType;
            var actTypeAfter = presAfter.Activity.ActivityType;
            if (actTypeBefore == actTypeAfter) return;
            string content;
            
            if (actTypeAfter == ActivityType.Streaming) //If start streaming
            {
                content = streamStartPhrase;
                var actInfo = presAfter.Activity.RichPresence;
                content.Replace("name", member.Nickname);
                content.Replace("game", actInfo.Application.Name);

                var embedBuilder = new DiscordEmbedBuilder();
                embedBuilder.SetAuthor(member);
                embedBuilder.SetTitle(actInfo.Details);
                embedBuilder.AddField("Игра", actInfo.Application.Name);
                embedBuilder.ImageUrl = actInfo.LargeImage.Url.ToString();
                embedBuilder.AddField("Ссылка", presAfter.Activity.StreamUrl);

                await notifyChannel.SendMessageAsync(content, embed: embedBuilder);

            }
            else if (actTypeBefore == ActivityType.Streaming) // If end streaming
            {
                content = streamEndPhrase;
                content.Replace("name", member.Nickname);
                await notifyChannel.SendMessageAsync(content);
            }


        }
    }
}
