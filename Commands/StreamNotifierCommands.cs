using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LOSCKeeper.Extensions;
using LOSCKeeper.Main;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Commands
{
    class StreamNotifierCommands : BaseCommandModule
    {
        #region Main Commands 
        [Command("setStream")]
        [Description("Выставляет текущий канал в качестве канала оповещений о стриме")]
        public async Task SetDefaultStreamChannel(CommandContext ctx)
        {
            var core = Core.Instance;
            core.StreamNotifier.NotifyChannel = ctx.Channel;
            await core.ConfigManager.SetNotifyChannel(NotifyChannelType.Stream, ctx.Channel);
        }
        [Command("SNsetRole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task SetStreamerRole(CommandContext ctx, DiscordRole role = null)
        {
            if(role == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !SNsetRole @roleName(упоминание роли)");
            }
            await Core.Instance.StreamNotifier.SetStreamerRole(role);
            await ctx.Channel.SendTempMessageAsync($"Роль {role.Mention} успешно установлена как роль транслятора");

        }
        #endregion

        #region Phrase Commands
        [Command("SNsetStart")]
        [Description("Задаёт УНИКАЛЬНУЮ фразу для начала стрима, работает только для пользователей с ролью транслятора !SNsetRole {Упоминание роли}")]
        public async Task SetStartPhrase(CommandContext ctx, 
            [Description("Пример фразы \"Здарова, я - name(Это слово автоматически заменится на ваше имя при оповещении)," +
            " будем играть в game(то же самое, что с именем, только игра)")]string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !SNsetEnd \"фраза\"");
            }
            await Core.Instance.StreamNotifier.SetStartPhrase(false, phrase, ctx);

        }

        [Command("SNsetEnd")]
        [Description("Задаёт УНИКАЛЬНУЮ фразу для конца стрима, работает только для пользователей с ролью транслятора !SNsetRole {Упоминание роли}")]

        public async Task SetEndPhrase(CommandContext ctx,
            [Description("Пример фразы \"Всем пока, с вами был name(Это слово автоматически заменится на ваше имя при оповещении)\", мой стрим закончился")] string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !SNsetEnd \"фраза\"");
            }
            await Core.Instance.StreamNotifier.SetEndPhrase(false, phrase, ctx);
            
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultStart")]
        [Description("Задаёт СТАНДАРТНУЮ фразу для начала стрима")]
        public async Task SetDefaultStartPhrase(CommandContext ctx,
            [Description("Пример фразы \"Здарова, я - name(Это слово автоматически заменится на имя стримера при оповещении)," +
            " будем играть в game(то же самое, что с именем, только игра)")]string phrase)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !SNsetDefaultStart \"фраза\"");
            }
            await Core.Instance.StreamNotifier.SetStartPhrase(true, phrase, ctx);

        }


        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultEnd")]
        [Description("Задаёт СТАНДАРТНУЮ фразу для конца стрима")]
        public async Task SetDefaultEndPhrase(CommandContext ctx, 
            [Description("Пример фразы \"Стрим на канале name(Это слово автоматически заменится на имя стримера при оповещении)\" закончился")]string phrase)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !SNsetDefaultEnd \"фраза\"");
            }
            await Core.Instance.StreamNotifier.SetEndPhrase(true, phrase, ctx);

        }
        #endregion


        
    }
}
