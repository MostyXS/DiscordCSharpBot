using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LOSCKeeper.Commands;
using LOSCKeeper.Notifications;
using System;

using System.Threading.Tasks;

namespace LOSCKeeper.Main
{
    public class Core
    {
        DiscordClient Client;
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public ConfigManager ConfigManager { get; private set; } = new ConfigManager();

        public GuildEvents GuildEvents { get; private set; }
        public RoleGranter RoleGranter { get; private set; }
        public StreamNotifier StreamNotifier { get; private set; }


        public static Core Instance { get; private set; }

       
        public async Task MainAsync()
        {

            Instance = this;

            Client = new DiscordClient(await ConfigManager.GetBotConfigAsync());
            await ConfigManager.CreateAsync(Client);

            await SubscribeToeventHandlers();

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });
            Commands = Client.UseCommandsNext(ConfigManager.GetCommandsConfig());

            RegisterAllCommands();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task SubscribeToeventHandlers()
        {
            var auditChannel = ConfigManager.GetNotifyChannel(NotifyChannelType.Audit);
            var defaultGuild = await ConfigManager.GetDefaultGuild(Client);

            var streamNotifyChannel = ConfigManager.GetNotifyChannel(NotifyChannelType.Stream);

            StreamNotifier = new StreamNotifier(defaultGuild, streamNotifyChannel, Client);
            RoleGranter = new RoleGranter(Client, defaultGuild);
            await RoleGranter.TryInitialize();
            
            GuildEvents = new GuildEvents(auditChannel, defaultGuild);
            GuildEvents.SubscribeToGuildEvents(Client);
        }

        private void RegisterAllCommands()
        {
            Commands.RegisterCommands<GuildEventsCommands>();
            Commands.RegisterCommands<RoleGranterCommands>();
            Commands.RegisterCommands<StreamNotifierCommands>();
        }
    }


}
