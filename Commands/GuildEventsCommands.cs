using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using LSSKeeper.Extensions;
using LSSKeeper.Main;
using System;
using System.Threading.Tasks;

namespace LSSKeeper.Commands
{
    class GuildEventsCommands : BaseCommandModule
    {
        public static GuildEvents GE { private get; set; }
        [Command("setAudit")]
        [Description("Выставляет текущий канал в качестве канала аудита")]
        public async Task SetDefaultAuditLogChannel(CommandContext ctx)
        {
            await GE.SetChannelAsync(ctx.Channel);
            await ctx.Channel.SendTempMessageAsync("Успешно установлен как канал аудита");

        }
        /*[Command("addResponse")]
        public async Task AddResponse(CommandContext ctx, string phrase, string answer)
        {

     
        }*/

        [Command("clear")]
        [Description("Удаляет канал и создаёт его клон, может быть использована только главой гильдии")]
        public async Task Clear(CommandContext ctx)
        {
            if (!ctx.Member.IsOwner) return;
            var c = await ctx.Channel.CloneAsync();
            Console.WriteLine(ctx.Channel.Id);
            Console.Write(c.Id);
            await ctx.Channel.DeleteAsync();
        }
    }
}
