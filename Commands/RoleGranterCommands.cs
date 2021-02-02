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
    class RoleGranterCommands : BaseCommandModule
    {
        #region Main Commands
        [Command("RGcreate")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Создаёт сообщение для выдачи ролей, добавлять роли в список командой !addRoleFromEmoji")]
        public async Task CreateRoleGranterMessage(CommandContext ctx,
            [Description("Заголовок")] string title = null,
            [Description("Описание")] string description = null,
            [Description("То, что написано перед ролями, по дефолту \"Привязка ролей\"")] string rolesFieldName = "Привязка ролей",
            string footer = "Для выдачи поставьте соответствующий смайлик")
        {
            if(title == null || description == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование " +
                    "!RGCreate \"Заголовок\" \"Описание\" \"Название поля с ролями\"(Необязательно) \"Футер\"(Необязательно");
                return;
            }
            await Core.Instance.RoleGranter.CreateRoleMessageAsync( ctx.Channel, title, description, rolesFieldName, footer);
        }

        [RequireOwner]
        [Description("Для дебага, только владелец бота")]
        [Command("rgreset")]
        public async Task Reset(CommandContext ctx)
        {
            await Core.Instance.RoleGranter.Reset();
        }


        [Command("RGaddRole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Добавляет роль в список ролей для выдачи через сообщение," +
            " обновляет если сообщение уже существует, то необходимости пересоздавать можно воспользоваться командами изменения")]
        public async Task AddRoleFromEmoji(CommandContext ctx, DiscordRole role = null, DiscordEmoji emoji = null, string description = null)
        {
            var tempMessageContent = "Неверное форматирование !RGaddRole {Упоминание роли} {Эмоджи} \"Описание\"";

            if(role == null || emoji == null || description == null)
            {
                await ctx.Channel.SendTempMessageAsync(tempMessageContent);
                return;
            }
            await ctx.Message.DeleteAsync();
            RoleAddResult result = await Core.Instance.RoleGranter.TryAddRoleByEmoji(role, emoji, description);
            switch (result)
            {
                case RoleAddResult.Succeed:
                    {
                        tempMessageContent = "Роль успешно добавлена";
                        break;
                    }
                case RoleAddResult.NoMessage:
                    {
                        tempMessageContent = "Сообщение с ролями не установлено, используйте !RGCreate";
                        break;
                    }
                case RoleAddResult.AlreadyHasEmoji:
                    {
                        tempMessageContent = "Роль привязанная к этой эмоции уже установлена";
                        break;
                    }
                case RoleAddResult.AlreadyHasRole:
                    {
                        tempMessageContent = "Такая роль уже установлена";
                        break;
                    }
            }
            await ctx.Channel.SendTempMessageAsync(tempMessageContent);
        }
        #endregion

        #region Edit Commands
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeTitle")]
        public async Task ChangeTitle(CommandContext ctx, string content)
        {
            if (content == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !RGchangeRFName \"Заголовок\"");
                return;
            }
            await Core.Instance.RoleGranter.ChangeEmbedAsync(title: content);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeDescription")]
        public async Task ChangeDescription(CommandContext ctx, string content)
        {
            if (content == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !RGchangeRFName \"Описание\"");
                return;
            }
            await Core.Instance.RoleGranter.ChangeEmbedAsync(description: content);
        }

        [Description("Меняет заголовок поля с ролями")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeRFName")]
        public async Task ChangeRFName(CommandContext ctx, string content = null)
        {
            if(content == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !RGchangeRFName \"Название поля с ролями\"");
                return;
            }
            await Core.Instance.RoleGranter.ChangeEmbedAsync(rfName: content);
        }
        
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeFooter")]
        public async Task ChangeFooter(CommandContext ctx, string content)
        {
            if (content == null)
            {
                await ctx.Channel.SendTempMessageAsync("Неверное форматирование !RGchangeRFName \"Футер\"");
                return;
            }
            await Core.Instance.RoleGranter.ChangeEmbedAsync(footer: content);
        }
        #endregion


    }
}
