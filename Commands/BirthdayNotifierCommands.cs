using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volodya.Extensions;
using Volodya.Modules;

namespace LSSKeeper.Commands
{
    public class BirthdayNotifierCommands : BaseCommandModule
    {
        public static BirthdayNotifier BNotifier { private get; set; }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("setBirthday")]
        public async Task SetBirthdayChannel(CommandContext ctx)
        {
            await BNotifier.SetBirthdayChannel(ctx.Channel);
            await ctx.Channel.SendMessageAsync("Успешно установлен как канал оповещений о днях рождения");
        }

        #region Birthdays Management
        [Command("BNaddInfo")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Добавляет оповещение о дне рождения определённого пользователя")]
        public async Task AddBirthdayInfo( CommandContext ctx, DiscordMember member = null, string name = null,
            int? day = null, int? month = null, int? year = null)
        {
            if (BNotifier.GetBirthdayChannel() != ctx.Channel) return;
            if(member == null || name == null ||
                (day == null || day<1 || day>31) ||
                (month == null || month <1 || month>12) 
                || (year == null || year > DateTime.Now.Year))
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование: " +
                    "!BNaddInfo @Упоминание Имя(У кого? пример: У Юры) День(1-31) Месяц(1-12) Год(Не больше текущего)");
                return;
            }
            if(await BNotifier.TryAddInfoAsync(member.Id, name, (int)day, (int)month, (int)year))
            {
                await ctx.Channel.SendTempMessageAsync($"Успешно добавлена информация о дне рождения пользователя {member.Mention}");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync($"Информация об этом пользователе уже существует");
            }
        }

        [Command("BNremoveInfo")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task RemoveBirthdayInfo(CommandContext ctx, DiscordMember member = null)
        {
            if (BNotifier.GetBirthdayChannel() != ctx.Channel) return;
            if (member == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование: !BNremoveInfo @Упоминание");
                return;
            }
            if (await BNotifier.TryRemoveInfoAsync(member.Id))
            {
                await ctx.Channel.SendTempMessageAsync($"Информация о дне рождения пользователя {member.Mention} успешно удалена");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync($"Информации о дне рождения пользователя {member} не найдено");
            }
        }

        [Command("AddBirthday")]
        public async Task AddUserBirthday(CommandContext ctx, string name,
            int? day = null, int? month = null, int? year = null)
        {
            if (BNotifier.GetBirthdayChannel() != ctx.Channel) return;
            if ( name == null ||
                (day == null || day < 1 || day > 31) ||
                (month == null || month < 1 || month > 12)
                || (year == null || year > DateTime.Now.Year))
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование: " +
                    "!AddBirthday Имя(У кого? пример: У Юры) День(1-31) Месяц(1-12) Год(Не больше текущего)");
                return;
            }

            if (await BNotifier.TryAddInfoAsync(ctx.Member.Id, name, (int)day, (int)month, (int)year))
            {
                await ctx.Channel.SendTempMessageAsync($"Информация о вашем дне рождения успешно добавлена");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync($"Информация о вашем дне рождения уже существует");
            }

        }

        [Command("RemoveBirthday")]
        public async Task RemoveUserBirthday(CommandContext ctx)
        {
            if (BNotifier.GetBirthdayChannel() != ctx.Channel) return;
            if(await BNotifier.TryRemoveInfoAsync(ctx.Member.Id))
            {
                await ctx.Channel.SendTempMessageAsync($"Информация о вашем дне рождения успешно удалена");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync("Информация о вашем дне рождения не найдена");
            }
        }
        #endregion
    }
}
