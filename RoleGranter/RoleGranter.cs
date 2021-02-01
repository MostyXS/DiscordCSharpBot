using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Commands
{
    public class RoleGranter
    {
        DiscordMessage rolesMessage;
        Dictionary<string, DiscordRole> rolesFromEmojis = new Dictionary<string, DiscordRole>();
        DiscordGuild defaultGuild;

        public RoleGranter(DiscordClient c, DiscordGuild defaultGuild)
        {
            this.defaultGuild = defaultGuild;
            c.MessageReactionAdded += TryGrantRoleAsync;
            c.MessageReactionRemoved += TryRevokeRoleAsync;
        }
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
            if (!rolesFromEmojis.TryGetValue(emjName, out role)) return false;
            return role != null;
        }

        public async Task CreateRoleMessageAsync(DiscordChannel channel, string title, string description, string rolesFieldName, string footer)
        {
            string rolesFieldValue = "Empty";
            if(rolesMessage != null)
            {
                rolesFieldValue = rolesMessage.Embeds[0].Fields[0].Value;
                await rolesMessage.DeleteAsync();
            }
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = description,
               
            };
            embedBuilder.AddField(rolesFieldName, rolesFieldValue);

            embedBuilder.AddField(footer, "_ _", true);
            var msg = await channel.SendMessageAsync(embed: embedBuilder);
            rolesMessage = msg;
            await Save();
        }

        public async Task Reset()
        {
            rolesFromEmojis = new Dictionary<string, DiscordRole>();
            if(rolesMessage != null)
            {
                await rolesMessage.DeleteAsync();
                rolesMessage = null;
            }
            await Save();
        }

        public async Task<bool> AddRoleByEmoji(DiscordRole role, DiscordEmoji emoji, string description)
        {
            if (rolesMessage == null) return false;

            if (rolesFromEmojis.ContainsValue(role)) return false;
            rolesFromEmojis.Add(emoji.Name, role);

            var oldEmbed = rolesMessage.Embeds[0];
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder(oldEmbed);
            var rolesField = builder.Fields[0];

            if(rolesField.Value.Equals("Empty"))
            {
                rolesField.Value = $"{role.Mention} - {emoji} - {description}";
            }
            else
            {
                rolesField.Value += $"\n{role.Mention} - {emoji} - {description}";
            }
            await rolesMessage.ModifyAsync(embed: builder.Build());
            await rolesMessage.CreateReactionAsync(emoji);
            await Save();
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
            embedBuilder.Fields[1].Name = footer != null ?footer : embedBuilder.Fields[1].Name;
            await rolesMessage.ModifyAsync(embed: embedBuilder.Build());
            await Save();
            
        }
        public async Task TryInitialize()
        {
            try
            {
                var json = string.Empty;
                using (var fs = File.OpenRead(ConfigNames.ROLEGRANTER))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();

                var rlj = JsonConvert.DeserializeObject<RoleGranterJson>(json);

                if (rlj.RolesMessageChannelId != null && rlj.RolesMessageId != null)
                {
                    rolesMessage = await defaultGuild.GetChannel((ulong)rlj.RolesMessageChannelId).GetMessageAsync((ulong)rlj.RolesMessageId);
                }
                var rawDict = rlj.RolesIdsFromEmojisIds;
                if (rawDict != null && rawDict.Count != 0)
                {
                    foreach (var kvp in rawDict)
                    {
                        var role = defaultGuild.GetRole(kvp.Value);
                        rolesFromEmojis.Add(kvp.Key, role);
                    }
                }
            }
            catch
            {
            }
        }

        private async Task Save()
        {
            
            Dictionary<string, ulong> rawRolesDictionary = new Dictionary<string, ulong>();
            foreach(var rFE in rolesFromEmojis)
            {
                rawRolesDictionary.Add(rFE.Key, rFE.Value.Id);
            }

            RoleGranterJson rlj = new RoleGranterJson();
            rlj.RolesIdsFromEmojisIds = rawRolesDictionary;
            rlj.RolesMessageId = rolesMessage?.Id;
            rlj.RolesMessageChannelId = rolesMessage?.ChannelId;
            
            string json = JsonConvert.SerializeObject(rlj);

            await File.WriteAllTextAsync(ConfigNames.ROLEGRANTER, json);

        }
    }
}
