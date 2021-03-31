using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Volodya.Extensions;
using Volodya.Main;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Volodya.Commands
{
    class AuditNotifierCommands : BaseCommandModule
    {
        public static AuditNotifier ANotifier { private get; set; }

        [RequirePermissions(Permissions.Administrator)]
        [Command("setAudit")]
        [Description("Выставляет текущий канал в качестве канала аудита")]
        public async Task SetDefaultAuditLogChannel(CommandContext ctx)
        {
            await ANotifier.SetChannelAsync(ctx.Channel);
            await ctx.Channel.SendTempMessageAsync("Успешно установлен как канал аудита");

        }
        #region ResponseCommands
        [RequirePermissions(Permissions.Administrator)]
        [Command("addResponse")]
        [Description("Добавляет ответ на определённую фразу или слово")]
        public async Task AddResponse(CommandContext ctx, string keyPhrase = null, string response = null)
        {
            if (keyPhrase == null || response == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !AddResponse \"ключевое слово/фраза\" \"Ответ на ключевое слово/фразу\"");
                return;
            }
            if (await ANotifier.TryAddResponseAsync(keyPhrase, response))
            {
                await ctx.Channel.SendTempMessageAsync("Ответ на фразу/слово успешно добавлен");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync("Такое слово/фраза уже есть в списке");
            }
        }
        [RequirePermissions(Permissions.Administrator)]
        [Command("removeResponse")]
        [Description("Удаляет ответ на уже заданную фразу")]
        public async Task RemoveResponse(CommandContext ctx, string keyPhrase = null)
        {
            if(keyPhrase == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !AddResponse \"ключевое слово/фраза\" ");
            }
            if(await ANotifier.TryRemoveResponseAsync(keyPhrase))
            {
                await ctx.Channel.SendTempMessageAsync("Ответ на фразу/слово успешно удалён");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync("Не удалось удалить ответ на фразу список фраз !getResponses");
            }
        }
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        [Command("getResponses")]
        [Description("Возвращает список ключевых фраз/слов")]
        public async Task GetResponses(CommandContext ctx)
        {
            var responses = ANotifier.GetResponses();
            if (responses.Length == 0) return;
            await ctx.Channel.SendMessageAsync("Список:\n" + string.Join('\n', responses));
        }
        #endregion
        #region Blacklist Commands
        [RequirePermissions(Permissions.Administrator)]
        [Command("blacklistWord")]
        [Description("Запрещает слово для никнейма на вход или изменение, при изменении на запрещённое слово - меняет, при заходе с запрещённым ником - банит")]
        public async Task BlacklistWord(CommandContext ctx, string word = null)
        {
            if(word == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !blacklistWord \"Слово\"");
                return;
            }
            if (await ANotifier.TryBlacklistWordAsync(word))
            {
                await ctx.Channel.SendTempMessageAsync($"Слово или фраза {word} успешно добавлено в список запрещённых");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync($"Слово или фраза {word} уже находится в списке запрещённых");
            }

        }

        [RequirePermissions(Permissions.Administrator)]
        [Command("deblacklistWord")]
        [Description("Удаляет слово из списка запрещённых")]

        public async Task DeblacklistWord(CommandContext ctx , string word = null)
        {
            if (word == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !deblacklistWord \"Слово\"");
                return;
            }
            if(await ANotifier.TryDeblacklistWordAsync(word))
            {
                await ctx.Channel.SendTempMessageAsync($"Слово {word} успешно удалено из списка запрещённых");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync($"Слово {word} не найдено в списке запрещённых !getBlacklistedWords");
            }

        }

        [Command("getBlacklistedWords")]
        [Description("Возвращает список запрещённых слов")]
        public async Task GetBlacklistedWords(CommandContext ctx)
        {
            var blwords = ANotifier.GetBlacklistedWords();
            await ctx.Channel.SendMessageAsync(content: "Список:\n" + string.Join('\n', blwords));

        }
        #endregion

    }
}
