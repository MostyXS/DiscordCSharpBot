using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LOSCKeeper.Main;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Commands
{
    class StreamNotifierCommands : BaseCommandModule
    {
        [Command("SNsetRole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task SetStreamerRole(CommandContext ctx, DiscordRole role)
        {
            await Core.Instance.StreamNotifier.SetStreamerRole(role);

        }
        [Command("SNsetStart")]
        public async Task SetStartPhrase(CommandContext ctx, string phrase)
        {
            await Core.Instance.StreamNotifier.SetStartPhrase(false, phrase, ctx.Member);
        }

        [Command("SNsetEnd")]
        public async Task SetEndPhrase(CommandContext ctx, string phrase)
        {
            await Core.Instance.StreamNotifier.SetEndPhrase(false, phrase, ctx.Member);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultStart")]
        public async Task SetDefaultStartPhrase(CommandContext ctx, string phrase)
        {
            await Core.Instance.StreamNotifier.SetStartPhrase(true, phrase);
        }
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultEnd")]
        public async Task SetDefaultEndPhrase(CommandContext ctx, string phrase)
        {
            await Core.Instance.StreamNotifier.SetEndPhrase(true, phrase);

        }

        [Command("setStream")]
        public async Task SetDefaultStreamChannel(CommandContext ctx)
        {
            var core = Core.Instance;
            core.StreamNotifier.NotifyChannel = ctx.Channel;
            await core.ConfigManager.SetNotifyChannel(NotifyChannelType.Stream, ctx.Channel);
        }
    }
}
