using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LSSKeeper.Main
{
    public abstract class KeeperModule
    {
        [JsonIgnore]
        protected Type CommandsType;
        [JsonIgnore]
        protected DiscordGuild DefaultGuild;
        [JsonIgnore]
        protected DiscordClient Client;

        public KeeperModule()
        {
        }


        public virtual void RegisterCommands(CommandsNextExtension commands)
        {
            commands.RegisterCommands(CommandsType);
        }
        public virtual async Task InitializeAsync(DiscordClient c, DiscordGuild g)
        {
            Client = c;
            DefaultGuild = g;
            await InitializeConfigAsync();
        }

        protected abstract Task InitializeConfigAsync();


        protected abstract Task SaveAsync();

    }
}
