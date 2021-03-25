using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Volodya.Commands;
using Volodya.Notifications;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volodya.Handlers;
using LSSKeeper.Commands;

namespace Volodya.Main
{
    public class Core
    {
        private DiscordClient _client;
        private CommandsNextExtension _commands;

        private DiscordGuild _defaultGuild;

        public event Action OnInitialize;


       
        public async Task MainAsync()
        {

            await AssignDefaultConfigurationsAsync();

            await SubscribeToEventHandlers();

            RegisterAllCommands();
            
            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task SubscribeToEventHandlers()
        {
            var rg = new RoleGranter();
            await rg.TryInitializeAsync(_client, _defaultGuild);
            RoleGranterCommands.RG = rg;

            var sn = new StreamNotifier();
            await sn.TryInitializeAsync(_client, _defaultGuild);
            StreamNotifierCommands.SN = sn;

            var ge  = new GuildEvents();
            ge.TryInitializeAsync(_client, _defaultGuild);
            GuildEventsCommands.GE = ge;
        }

        private void RegisterAllCommands()
        {
            _commands.RegisterCommands<GuildEventsCommands>();
            _commands.RegisterCommands<RoleGranterCommands>();
            _commands.RegisterCommands<StreamNotifierCommands>();
            _commands.RegisterCommands<CommonCommands>();
        }
        private async Task AssignDefaultConfigurationsAsync()
        {
            var jsonString = await File.ReadAllTextAsync("config.json");
            var defaultConfig = JsonConvert.DeserializeObject<DefaultJson>(jsonString);

            _client = new DiscordClient(new DiscordConfiguration()
            {
                Token = defaultConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                MessageCacheSize = 131072
                
            });
            _commands = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { defaultConfig.Prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true
            });
            _defaultGuild = await _client.GetGuildAsync(defaultConfig.GuildId);
            OnInitialize?.Invoke();
            
        }

        public void AddBirthdayNotifierModule(BirthdayNotifier bdNotifier)
        {
            bdNotifier.TryInitializeAsync(_defaultGuild);
            BirthdayNotifierCommands.BN = bdNotifier;
            _commands.RegisterCommands<BirthdayNotifierCommands>();
            
        }
    }


}
