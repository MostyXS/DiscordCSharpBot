using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Volodya.Extensions;
using Volodya.Main;
using System.Threading.Tasks;

namespace Volodya.Commands
{
    class RoleGranterCommands : BaseCommandModule
    {
        public static RoleGranter RGranter { private get; set; }

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
                await ctx.Channel.SendMessageAsync("Неверное форматирование " +
                    "!RGCreate \"Заголовок\" \"Описание\" \"Название поля с ролями\"(Необязательно) \"Футер\"(Необязательно");
                return;
            }
            await RGranter.CreateRoleMessageAsync(ctx, title, description, rolesFieldName, footer);
        }

        [Command("RGaddRole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Добавляет роль в список ролей для выдачи через сообщение," +
            " обновляет если сообщение уже существует, то необходимости пересоздавать можно воспользоваться командами изменения")]
        public async Task AddRoleFromEmoji(CommandContext ctx, DiscordRole role = null, DiscordEmoji emoji = null, string description = null)
        {
            var tempMessageContent = "Неверное форматирование !RGaddRole {Упоминание роли через @} {Эмоджи} \"Описание\"(Без фигурных скобок)";

            if(role == null || emoji == null || description == null)
            {
                await ctx.Channel.SendMessageAsync(tempMessageContent);
                return;
            }
            
            RoleAddResult result = await RGranter.TryAddRoleAsync(ctx.Client, role, emoji, description);
            switch (result)
            {
                case RoleAddResult.Succeed:
                    {
                        tempMessageContent = "Роль успешно добавлена и готова для выдачи";
                        break;
                    }
                case RoleAddResult.SucceedWithoutMessage:
                    {
                        tempMessageContent = "Роль успешно добавлена, для создания сообщения с выдачей используйте !rgcreate";
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
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGremoveRole")]
        public async Task RemoveRoleFromEmoji(CommandContext ctx, DiscordRole role)
        {
            if(role == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !RGremoveRole {Упоминание роли через @}(Без фигурных скобок)");
            }
            if(await RGranter.TryRemoveRoleAsync(ctx.Client, role))
            {
                await ctx.Channel.SendTempMessageAsync("Роль успешно удалена");
            }
            else
            {
                await ctx.Channel.SendTempMessageAsync("Такой роли нет в списке");
            }
        }
        #endregion

        #region Edit Commands
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeTitle")]
        public async Task ChangeTitle(CommandContext ctx, string content)
        {
            if (content == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !RGchangeRFName \"Заголовок\"");
                return;
            }
            await RGranter.ChangeEmbedAsync(title: content);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeDescription")]
        public async Task ChangeDescription(CommandContext ctx, string content)
        {
            if (content == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !RGchangeRFName \"Описание\"");
                return;
            }
            await RGranter.ChangeEmbedAsync(description: content);
        }

        [Description("Меняет заголовок поля с ролями")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeRFName")]
        public async Task ChangeRFName(CommandContext ctx, string content = null)
        {
            if(content == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !RGchangeRFName \"Название поля с ролями\"");
                return;
            }
            await RGranter.ChangeEmbedAsync(rfName: content);
        }
        
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeFooter")]
        public async Task ChangeFooter(CommandContext ctx, string content)
        {
            if (content == null)
            {
                await ctx.Channel.SendMessageAsync("Неверное форматирование !RGchangeRFName \"Футер\"");
                return;
            }
            await RGranter.ChangeEmbedAsync(footer: content);
        }
        #endregion

    }
}
