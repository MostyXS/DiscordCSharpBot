using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volodya.Handlers;

namespace LSSKeeper.Commands
{
    public class BirthdayNotifierCommands : BaseCommandModule
    {
        public static BirthdayNotifier BN { private get; set; }

        [Command("BNaddInfo")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        private async Task AddBirthdayInfo( CommandContext ctx, DiscordM mem )
        {
            if()

            if(BN.AddInfo())
        }

    }
}
