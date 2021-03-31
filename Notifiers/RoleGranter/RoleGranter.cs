using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Volodya.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSSKeeper.Main;

namespace Volodya.Commands
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class RoleGranter : KeeperModule
    {
        [JsonIgnore]
        private DiscordMessage rolesMessage;

        private List<RoleInfo> rolesInfo = new List<RoleInfo>();
        private ulong? channelId;
        private ulong? rolesMessageId;

        #region Module Methods
        public override async Task InitializeAsync(DiscordClient c, DiscordGuild guild)
        {
            await base.InitializeAsync(c, guild);
            Client.MessageReactionAdded += GrantRoleAsync;
            Client.MessageReactionRemoved += RevokeRoleAsync;
        }
        public override void RegisterCommands(CommandsNextExtension commands)
        {
            RoleGranterCommands.RGranter = this;
            CommandsType = typeof(RoleGranterCommands);
            base.RegisterCommands(commands);
        }
        protected override async Task InitializeConfigAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(ConfigNames.ROLEGRANTER);

                var rgj = JsonConvert.DeserializeObject<RoleGranter>(json);
                channelId = rgj.channelId;
                rolesMessageId = rgj.rolesMessageId;
                if (channelId != null && rolesMessageId != null)
                {
                    rolesMessage = await DefaultGuild.GetChannel((ulong)rgj.channelId).GetMessageAsync((ulong)rgj.rolesMessageId);
                }
                if (rgj.rolesInfo != null)
                    rolesInfo = rgj.rolesInfo;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Не удалось инициализировать конфиг выдачи ролей " + e.Message);
            }
        }

        protected override async Task SaveAsync()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this);
                await File.WriteAllTextAsync(ConfigNames.ROLEGRANTER, json);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Не удалось сохранить конфиг выдачи ролей " + e.Message);
            }

        }
        #endregion

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
                await msg.CreateReactionAsync(r.EmojiName.ToEmoji(ctx.Client));
            }
            channelId = msg.ChannelId;
            rolesMessageId = msg.Id;
            rolesMessage = msg;
            await SaveAsync();
        }

        public async Task<RoleAddResult> TryAddRoleAsync(DiscordClient c, DiscordRole role, DiscordEmoji emoji, string desc)
        {

            if (rolesInfo.Any((x) => x.RoleID == role.Id)) return RoleAddResult.AlreadyHasRole;
            else if (rolesInfo.Any((x) => x.EmojiName == emoji.Name)) return RoleAddResult.AlreadyHasEmoji;

            RoleInfo ri;
            ri.RoleID = role.Id;
            ri.EmojiName = emoji.Name;
            ri.Description = desc;
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
            if (!rolesInfo.Any((x) => x.RoleID == role.Id)) return false;
            var info = rolesInfo.Where((x) => x.RoleID == role.Id).First();
            rolesInfo.Remove(info);
            await SaveAsync();

            if (rolesMessage == null) return true;

            var embedBuilder = new DiscordEmbedBuilder(rolesMessage.Embeds[0]);
            embedBuilder.Fields[0].Value = GetRolesString(c);

            await rolesMessage.ModifyAsync(embed: embedBuilder.Build());
            await rolesMessage.DeleteReactionAsync(info.EmojiName.ToEmoji(c), c.CurrentUser);
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
            rolesMessage = await rolesMessage.ModifyAsync(embed: embedBuilder.Build());

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
        private async Task GrantRoleAsync(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            var member = await DefaultGuild.GetMemberAsync(e.User.Id);
            if (member == null || member.IsBot) return;
            DiscordRole role;
            if (!TryGetReactionRole(e.Message, e.Emoji.Name, out role)) return;
            await member.GrantRoleAsync(role);
        }

        private async Task RevokeRoleAsync(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {

            var member = await DefaultGuild.GetMemberAsync(e.User.Id);
            if (member == null) return;
            DiscordRole role;
            if (!TryGetReactionRole(e.Message, e.Emoji.Name, out role)) return;
            await member.RevokeRoleAsync(role);
        }

        private bool TryGetReactionRole(DiscordMessage msg, string emjName, out DiscordRole role)
        {
            role = null;
            if (msg.Id != rolesMessage?.Id) return false;
            role = DefaultGuild.GetRole(rolesInfo.Where((x) => x.EmojiName == emjName).First().RoleID);
            return role != null;
        }
        #endregion

        #region Private Methods
        private string GetRolesString(DiscordClient c)
        {
            if (rolesInfo.Count == 0) return "Empty";

            StringBuilder builder = new StringBuilder("");

            foreach (var i in rolesInfo)
            {
                var role = DefaultGuild.GetRole(i.RoleID);
                builder.Append($"{role.Mention} - {i.EmojiName.ToEmoji(c)} - {i.Description}\n");
            }
            return builder.ToString();
        }
        #endregion
        struct RoleInfo
        {
            public ulong RoleID;
            public string EmojiName;
            public string Description;
        }
    }
}
