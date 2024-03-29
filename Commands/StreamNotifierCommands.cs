﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Volodya.Extensions;
using Volodya.Main;
using Volodya.Notifications;
using System.Threading.Tasks;

namespace Volodya.Commands
{
    public class StreamNotifierCommands : BaseCommandModule
    {
        public static StreamNotifier SNotifier { private get; set; }

        #region Main Commands 
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetChannel")]
        [Description("Выставляет текущий канал в качестве канала оповещений о стриме")]
        public async Task SetDefaultStreamChannel(CommandContext ctx)
        {
            await SNotifier.SetChannelAsync(ctx.Channel);
            await ctx.Channel.SendTempMessageAsync("Успешно установлен как канал для оповещений о стриме");
        }

        [Command("SNsetRole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task SetStreamerRole(CommandContext ctx, DiscordRole role = null)
        {
            if(role == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !SNsetRole @roleName(упоминание роли)");
                return;
            }
            await SNotifier.SetStreamerRole(role);
            await ctx.Channel.SendTempMessageAsync($"Роль {role.Mention} успешно установлена как роль транслятора");

        }
        #endregion

        #region Streamer's Commands
        
        [Command("SNsetStart")]
        [Description("Задаёт УНИКАЛЬНУЮ фразу для начала стрима, работает только для пользователей с ролью транслятора !SNsetRole {Упоминание роли}")]
        public async Task SetStartPhrase(CommandContext ctx, 
            [Description("Пример фразы \"Здарова, я - name(Это слово автоматически заменится на ваше имя при оповещении)," +
            " будем играть в game(то же самое, что с именем, только игра)")]string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !SNsetEnd \"фраза\"");
            }
            await SNotifier.SetStreamerInfo(ctx, phrase, Notifications.StreamerInfoType.StartPhrase);

        }

        [Command("SNsetImage")]
        [Description("Задаёт УНИКАЛЬНУЮ картинку для начала стрима, работает только для пользователей с ролью транслятора !SNsetRole {Упоминание роли}")]
        public async Task SetImageUrl(CommandContext ctx, string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !SNsetImage {ссылка на картинку}(без фигурных скобок)");
            }
            await SNotifier.SetStreamerInfo(ctx, phrase, Notifications.StreamerInfoType.ImageUrl);
        }

        [Command("SNsetEnd")]
        [Description("Задаёт УНИКАЛЬНУЮ фразу для конца стрима, работает только для пользователей с ролью транслятора !SNsetRole {Упоминание роли}")]
        public async Task SetEndPhrase(CommandContext ctx,
            [Description("Пример фразы \"Всем пока, с вами был name(Это слово автоматически заменится на ваше имя при оповещении)\", мой стрим закончился")] string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !SNsetEnd \"фраза\"");
            }
            await SNotifier.SetStreamerInfo(ctx, phrase, Notifications.StreamerInfoType.EndPhrase);
        }
        #endregion

        #region Default Commands
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultStart")]
        [Description("Задаёт СТАНДАРТНУЮ фразу для начала стрима")]
        public async Task SetDefaultStartPhrase(CommandContext ctx,
            [Description("Пример фразы \"Здарова, я - name(Это слово автоматически заменится на имя стримера при оповещении)," +
            " будем играть в game(то же самое, что с именем, только игра)")]string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !SNsetDefaultStart \"фраза\"");
            }
            var msgContent = await SNotifier.SetDefaultStreamerInfo(phrase, Notifications.StreamerInfoType.StartPhrase);
            await ctx.Channel.SendTempMessageAsync(msgContent);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultImage")]
        [Description("Задаёт СТАНДАРТНУЮ картинку для начала стрима")]
        public async Task SetDefaultImageUrl(CommandContext ctx, string imageUrl = null)
        {
            if (imageUrl == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !SNsetDefaultImage {ссылка на картинку}(без фигурных скобок)");
            }
            var msgContent = await SNotifier.SetDefaultStreamerInfo(imageUrl, Notifications.StreamerInfoType.ImageUrl);
            await ctx.Channel.SendTempMessageAsync(msgContent);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("SNsetDefaultEnd")]
        [Description("Задаёт СТАНДАРТНУЮ фразу для конца стрима")]
        public async Task SetDefaultEndPhrase(CommandContext ctx, 
            [Description("Пример фразы \"Стрим на канале name(Это слово автоматически заменится на имя стримера при оповещении)\" закончился")]string phrase = null)
        {
            if (phrase == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !SNsetDefaultEnd \"фраза\"");
            }
            var msgContent = await SNotifier.SetDefaultStreamerInfo(phrase, Notifications.StreamerInfoType.EndPhrase);
            await ctx.Channel.SendTempMessageAsync(msgContent);

        }
        #endregion


    }
}
