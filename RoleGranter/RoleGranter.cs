﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSSKeeper.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper.Commands
{
    public class RoleGranter
    {
        DiscordGuild defaultGuild;

        DiscordMessage rolesMessage;
        List<RoleInfo> rolesInfo = new List<RoleInfo>();

        public RoleGranter(DiscordClient c, DiscordGuild defaultGuild)
        {
            this.defaultGuild = defaultGuild;
            c.MessageReactionAdded += TryGrantRoleAsync;
            c.MessageReactionRemoved += TryRevokeRoleAsync;
        }
        private string GetRolesString(DiscordClient c)
        {
            if (rolesInfo.Count == 0) return "Empty";

            StringBuilder builder = new StringBuilder("");

            foreach (var i in rolesInfo)
            {
                var role = defaultGuild.GetRole(i.roleId);
                builder.Append($"{role.Mention} - {i.emojiName.ToEmoji(c)} - {i.desc}\n");
            }
            return builder.ToString();
        }

        #region Commands
        public async Task CreateRoleMessageAsync(CommandContext ctx, string title, string description, string rolesFieldName, string footer)
        {
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = description,

            };
            embedBuilder.AddField(rolesFieldName, GetRolesString(ctx.Client));
            embedBuilder.AddField(footer, "_ _", true);
            var msg = await ctx.Channel.SendMessageAsync(embed: embedBuilder);
            foreach (var r in rolesInfo)
            {
                await msg.CreateReactionAsync(r.emojiName.ToEmoji(ctx.Client));
            }
            rolesMessage = msg;
            await SaveAsync();
        }

        public async Task<RoleAddResult> TryAddRoleAsync(DiscordClient c, DiscordRole role, DiscordEmoji emoji, string desc)
        {
            
            if (rolesInfo.Any((x) => x.roleId == role.Id)) return RoleAddResult.AlreadyHasRole;
            else if (rolesInfo.Any((x) => x.emojiName == emoji.Name)) return RoleAddResult.AlreadyHasEmoji;

            RoleInfo ri;
            ri.roleId = role.Id;
            ri.emojiName = emoji.Name;
            ri.desc = desc;
            rolesInfo.Add(ri);
            await SaveAsync();

            if (rolesMessage == null) return RoleAddResult.SucceedWithoutMessage;

            var oldEmbed = rolesMessage.Embeds[0];
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder(oldEmbed);
            builder.Fields[0].Value = GetRolesString(c);

            await rolesMessage.ModifyAsync(embed: builder.Build());
            await rolesMessage.CreateReactionAsync(emoji);

            return RoleAddResult.Succeed;
        }
        public async Task<bool> TryRemoveRoleAsync(DiscordClient c, DiscordRole role)
        {
            if (!rolesInfo.Any((x) => x.roleId == role.Id)) return false;
            var info = rolesInfo.Where((x) => x.roleId == role.Id).First();
            rolesInfo.Remove(info);
            await SaveAsync();

            if (rolesMessage == null) return true;

            var embedBuilder = new DiscordEmbedBuilder(rolesMessage.Embeds[0]);
            embedBuilder.Fields[0].Value = GetRolesString(c);

            await rolesMessage.ModifyAsync(embed: embedBuilder.Build());
            await rolesMessage.DeleteReactionAsync(info.emojiName.ToEmoji(c), c.CurrentUser);
            return true;

        }
        
        public async Task ChangeEmbedAsync(string title = null, string description = null, string rfName = null, string footer = null)
        {
            if (rolesMessage == null) return;
            var oldEmbed = rolesMessage.Embeds[0];
            var embedBuilder = new DiscordEmbedBuilder(oldEmbed);
            embedBuilder.Title = title != null ? title : embedBuilder.Title;
            embedBuilder.Description = description != null ? description : embedBuilder.Description;
            embedBuilder.Fields[0].Name = rfName != null ? rfName : embedBuilder.Fields[0].Name;
            embedBuilder.Fields[1].Name = footer != null ? footer : embedBuilder.Fields[1].Name;
            await rolesMessage.ModifyAsync(embed: embedBuilder.Build());
        }

        public async Task Reset()
        {
            rolesInfo.Clear();
            if (rolesMessage != null)
            {
                try
                {
                    await rolesMessage.DeleteAsync();
                }
                catch
                {

                }
                rolesMessage = null;
            }
            File.Delete(ConfigNames.ROLEGRANTER);
        }


        #endregion

        #region Events
        

        private async Task TryGrantRoleAsync(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            var member = await defaultGuild.GetMemberAsync(e.User.Id);
            if (member == null || member.IsBot) return;
            DiscordRole role;
            if (!TryGetReactionRole(e.Message, e.Emoji.Name, out role)) return;
            await member.GrantRoleAsync(role);
        }

        private async Task TryRevokeRoleAsync(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {

            var member = await defaultGuild.GetMemberAsync(e.User.Id);
            if (member == null) return;
            DiscordRole role;
            if (!TryGetReactionRole(e.Message, e.Emoji.Name, out role)) return;
            await member.RevokeRoleAsync(role);
        }

        private bool TryGetReactionRole(DiscordMessage msg, string emjName, out DiscordRole role)
        {
            role = null;
            if (msg.Id != rolesMessage?.Id) return false;
            role = defaultGuild.GetRole(rolesInfo.Where((x) => x.emojiName == emjName).First().roleId);
            return role != null;
        }
        #endregion

        #region ConfigManagement
        public async Task TryInitializeAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(ConfigNames.ROLEGRANTER);

                var rgj = JsonConvert.DeserializeObject<RGJson>(json);

                if (rgj.channelId != null && rgj.messageId != null)
                {
                    rolesMessage = await defaultGuild.GetChannel((ulong)rgj.channelId).GetMessageAsync((ulong)rgj.messageId);
                }
                if (rgj.rolesInfo != null)
                    rolesInfo = rgj.rolesInfo;
            }
            catch
            {
            }
        }
        private async Task SaveAsync()
        {
            RGJson rgj = new RGJson();
            rgj.rolesInfo = rolesInfo;
            rgj.messageId = rolesMessage?.Id;
            rgj.channelId = rolesMessage?.ChannelId;
            string json = JsonConvert.SerializeObject(rgj);
            await File.WriteAllTextAsync(ConfigNames.ROLEGRANTER, json);

        }
        #endregion
        #region Inner Structs

        struct RoleInfo
        {
            public ulong roleId;
            public string emojiName;
            public string desc;
        }

        struct RGJson
        {
            public ulong? channelId;
            public ulong? messageId;
            public List<RoleInfo> rolesInfo;

        }

        #endregion
    }
}
