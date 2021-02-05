using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper.Main
{
    public class ConfigManager
    {
        Dictionary<NotifyChannelType, DiscordChannel> notifyChannels = new Dictionary<NotifyChannelType, DiscordChannel>();
        
        DefaultJson defaultConfig;

        /// <summary>
        /// Method should execute after client gets bot config
        /// </summary>
        /// <param name="c"></param>
        public async Task CreateAsync(DiscordClient c)
        {
            var g = await GetDefaultGuild(c);
            await AssignKnownChannelsAsync(g);

        }
        public async Task SetNotifyChannel(NotifyChannelType type, DiscordChannel channel)
        {
            string fileName = ConfigNames.DEFAULT;
            
            string file = await File.ReadAllTextAsync(fileName);
            JObject json = null;
            try
            {
                json = JObject.Parse(file);
                json[type.ToString()] = channel.Id;
            }
            catch
            {
                if (json == null)
                    json = new JObject();
                json.Add(type.ToString(), channel.Id);
            }
            notifyChannels[type] = channel;
            await File.WriteAllTextAsync(fileName, json.ToString());
            var msg = await channel.SendMessageAsync($"Успешно установлен как канал типа {type}");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await msg.DeleteAsync();
        }
        #region Getters
        public async Task<DiscordGuild> GetDefaultGuild(DiscordClient c)
        {
            return await c.GetGuildAsync(defaultConfig.GuildId);
        }
        

        public async Task<DiscordConfiguration> GetBotConfigAsync()
        {
            await AssignDefaultConfig(); 

            return new DiscordConfiguration()
            {
                
                Token = defaultConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                MessageCacheSize = 131072,

            };
        }
        public CommandsNextConfiguration GetCommandsConfig()
        {
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { defaultConfig.Prefix },
                EnableDms = false, //direct messages(Not in channel chat)
                EnableMentionPrefix = true,
                DmHelp = true
            };
            return commandsConfig;
        }

        #endregion
        public DiscordChannel GetNotifyChannel(NotifyChannelType type)
        {
            DiscordChannel c;
            notifyChannels.TryGetValue(type, out c);
            return c;
        }
        #region Private Methods
        private async Task AssignDefaultConfig()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            defaultConfig = JsonConvert.DeserializeObject<DefaultJson>(json);

            
        }
        private async Task AssignKnownChannelsAsync(DiscordGuild guild)
        {
            if (!File.Exists(ConfigNames.DEFAULT))
                File.Create(ConfigNames.DEFAULT);

            foreach (var cType in (NotifyChannelType[])Enum.GetValues(typeof(NotifyChannelType)))
            {
                DiscordChannel channel;
                notifyChannels.TryGetValue(cType, out channel);

                
                
                string file = await File.ReadAllTextAsync(ConfigNames.DEFAULT);
                JObject json = null;
                try
                {
                    json = JObject.Parse(file);
                    if (json == null) return;
                }
                catch
                {
                    Console.WriteLine("Json Config пуст или неверно форматирован");
                    return;
                }

                var cId = json[cType.ToString()]?.ToString();
                if (string.IsNullOrEmpty(cId))
                {
                    Console.WriteLine($"Стандартный канал типа {cType} не задан, используйте команду !set{cType}");
                    return;

                }
                channel = guild.GetChannel(Convert.ToUInt64(cId));
                if (channel == null)
                {
                    Console.WriteLine($"Конфиг содержит неверное ID канала типа {cType}, попробуйте переназначить с помощью команды !set{cType}");
                    return;
                }
                notifyChannels[cType] = channel;

            }
        }
        #endregion
    }
}
