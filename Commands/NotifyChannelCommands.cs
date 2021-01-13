using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace LOSCKeeper.Commands
{
    class NotifyChannelCommands : BaseCommandModule
    {
        [Command("setAudit")]
        public async Task SetDefaultAuditLogChannel(CommandContext ctx)
        {
            var core = Core.Instance;
            var c = ctx.Channel;
            core.GuildEvents.AuditChannel = ctx.Channel;
            await core.ConfManager.SetNotifyChannel(NotifyChannelType.Audit, ctx.Channel);

        }
        [Command("setStream")]
        public async Task SetDefaultStreamChannel(CommandContext ctx)
        {
            var core = Core.Instance;
            var c = ctx.Channel;
            core.GuildEvents.AuditChannel = ctx.Channel;
            await core.ConfManager.SetNotifyChannel(NotifyChannelType.Audit, ctx.Channel);
        }
    }
}
