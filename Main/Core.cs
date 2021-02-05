using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSSKeeper.Commands;
using LSSKeeper.Notifications;
using System;

using System.Threading.Tasks;

namespace LSSKeeper.Main
{
    public class Core
    {
        DiscordClient Client;
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public ConfigManager ConfigManager { get; private set; } = new ConfigManager();

        public GuildEvents GuildEvents { get; private set; }

        public static Core Instance { get; private set; }

       
        public async Task MainAsync()
        {
            Instance = this;

            Client = new DiscordClient(await ConfigManager.GetBotConfigAsync());
            await ConfigManager.CreateAsync(Client);

            await SubscribeToEventHandlers();

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });
            Commands = Client.UseCommandsNext(ConfigManager.GetCommandsConfig());

            RegisterAllCommands();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task SubscribeToEventHandlers()
        {
            var defaultGuild = await ConfigManager.GetDefaultGuild(Client);

            var rg = new RoleGranter(Client, defaultGuild);
            await rg.TryInitializeAsync();
            RoleGranterCommands.RG = rg;

            var streamNotifyChannel = ConfigManager.GetNotifyChannel(NotifyChannelType.Stream);
            var sn = new StreamNotifier(Client, defaultGuild, streamNotifyChannel);
            await sn.TryInitializeAsync();
            StreamNotifierCommands.SN = sn;

            var auditChannel = ConfigManager.GetNotifyChannel(NotifyChannelType.Audit);
            GuildEvents = new GuildEvents(auditChannel, defaultGuild);
            GuildEvents.SubscribeToGuildEvents(Client);
        }

        private void RegisterAllCommands()
        {
            Commands.RegisterCommands<GuildEventsCommands>();
            Commands.RegisterCommands<RoleGranterCommands>();
            Commands.RegisterCommands<StreamNotifierCommands>();
            Commands.RegisterCommands<CommonCommands>();
        }
    }


}
