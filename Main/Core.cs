using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSSKeeper.Commands;
using LSSKeeper.Notifications;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper.Main
{
    public class Core
    {
        DiscordClient client;
        public static CommandsNextExtension Commands { get; private set; }

        static DiscordGuild defaultGuild;


       
        public async Task MainAsync()
        {


            await AssignDefaultConfigurationsAsync();

            await SubscribeToEventHandlers();

            RegisterAllCommands();

            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task SubscribeToEventHandlers()
        {

            var rg = new RoleGranter();
            await rg.TryInitializeAsync(client, defaultGuild);
            RoleGranterCommands.RG = rg;

            var sn = new StreamNotifier();
            await sn.TryInitializeAsync(client, defaultGuild);
            StreamNotifierCommands.SN = sn;

            var ge  = new GuildEvents();
            ge.TryInitializeAsync(client, defaultGuild);
            GuildEventsCommands.GE = ge;
        }

        private void RegisterAllCommands()
        {
            Commands.RegisterCommands<GuildEventsCommands>();
            Commands.RegisterCommands<RoleGranterCommands>();
            Commands.RegisterCommands<StreamNotifierCommands>();
            Commands.RegisterCommands<CommonCommands>();
        }
        private async Task AssignDefaultConfigurationsAsync()
        {
            var jsonString = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                jsonString = await sr.ReadToEndAsync().ConfigureAwait(false);
            var defaultConfig = JsonConvert.DeserializeObject<DefaultJson>(jsonString);

            client = new DiscordClient(new DiscordConfiguration()
            {
                Token = defaultConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                MessageCacheSize = 131072
                
            });
            Commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { defaultConfig.Prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true
            });
            defaultGuild = await client.GetGuildAsync(defaultConfig.GuildId);
            
        }
    }


}
