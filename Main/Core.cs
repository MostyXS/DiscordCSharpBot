using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LOSCKeeper.Commands;
using LOSCKeeper.Main;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

using System.Threading.Tasks;

namespace LOSCKeeper
{
    public class Core
    {
        DiscordClient Client;
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public ConfigManager ConfManager { get; private set; } = new ConfigManager();
        public GuildEvents GuildEvents { get; private set; }

        public static Core Instance { get; private set; }

       
        




        public async Task MainAsync()
        {

            Instance = this;
            Client = new DiscordClient(await ConfManager.GetBotConfigAsync());
            await ConfManager.CreateAsync(Client);

            await SubscribeToGuildEvents();

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });
            Commands = Client.UseCommandsNext(ConfManager.GetCommandsConfig());


            RegisterAllCommands();

            await Client.ConnectAsync();

            await Task.Delay(-1);

        }

        private async Task SubscribeToGuildEvents()
        {
            var auditChannel = ConfManager.GetNotifyChannel(NotifyChannelType.Audit);
            var defaultGuild = await ConfManager.GetDefaultGuild(Client);
            GuildEvents = new GuildEvents(auditChannel, defaultGuild);
            GuildEvents.SubscribeToGuildEvents(Client);
        }

        private void RegisterAllCommands()
        {
            Commands.RegisterCommands<FunCommands>(); 
            Commands.RegisterCommands<TeamCommands>();
            Commands.RegisterCommands<NotifyChannelCommands>();
        }
    }


}
