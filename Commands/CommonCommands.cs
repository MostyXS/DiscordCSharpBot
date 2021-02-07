using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LSSKeeper.Commands
{
    class CommonCommands : BaseCommandModule
    {
        [Command("author")]
        public async Task PrintAuthor(CommandContext ctx)
        {
            DiscordUser[] owners = ctx.Client.CurrentApplication.Owners.ToArray();
            await ctx.Channel.SendMessageAsync(owners[0].Mention);
        }
        [RequireOwner]
        [Command("getGuildId")]
        public async Task GetGuildId(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(ctx.Guild.Id.ToString());
        }


    }
}
