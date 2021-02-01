using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using LOSCKeeper.Main;
using System;
using System.Threading.Tasks;

namespace LOSCKeeper.Commands
{
    class GuildEventsCommands : BaseCommandModule
    {
        [Command("setAudit")]
        public async Task SetDefaultAuditLogChannel(CommandContext ctx)
        {
            var core = Core.Instance;
            core.GuildEvents.AuditChannel = ctx.Channel;
            await core.ConfigManager.SetNotifyChannel(NotifyChannelType.Audit, ctx.Channel);

        }
        /*[Command("addResponse")]
        public async Task AddResponse(CommandContext ctx, string phrase, string answer)
        {

     
        }*/

        [Command("clear")]
        public async Task Clear(CommandContext ctx)
        {
            if (!ctx.Member.IsOwner) return;
            var c = await ctx.Channel.CloneAsync();
            Console.WriteLine(ctx.Channel.Id);
            Console.Write(c.Id);
            await ctx.Channel.DeleteAsync();
        }
    }
}
