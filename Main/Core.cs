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
using Volodya.Modules;
using LSSKeeper.Commands;
using LSSKeeper.Main;

namespace Volodya.Main
{
    public class Core
    {
        private DiscordClient _client;
        public static CommandsNextExtension Commands { get; private set; }

        private static DiscordGuild _defaultGuild;

        public event Action OnInitialize;

        private KeeperModule[] modules = new KeeperModule[] { new RoleGranter(), new StreamNotifier(), new AuditNotifier() };
        public async Task MainAsync()
        {
            OnInitialize += async () => await InitializeModules();

            await InitializeConfig();
            Commands.RegisterCommands<CommonCommands>();
            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task InitializeModules()
        {
            foreach(var module in modules)
            {
                await AddModuleAsync(module);
            }
        }

        private async Task InitializeConfig()
        {
            var jsonString = await File.ReadAllTextAsync("config.json");
            var _config = JsonConvert.DeserializeObject<DefaultJson>(jsonString);

            _client = new DiscordClient(new DiscordConfiguration()
            {
                Token = _config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                MessageCacheSize = 131072
                
            });
            Commands = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { _config.Prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true
            });
            _defaultGuild = await _client.GetGuildAsync(_config.GuildId);
            OnInitialize?.Invoke();
        }
        public async Task AddModuleAsync(KeeperModule module)
        {
            await module.InitializeAsync(_client, _defaultGuild);
            module.RegisterCommands(Commands);

        }
    }
    

}
