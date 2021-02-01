using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LOSCKeeper.Main;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Commands
{
    class RoleGranterCommands : BaseCommandModule
    {

        [Command("RGcreate")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Создаёт сообщение для выдачи ролей, добавлять роли в список командой !addRoleFromEmoji")]
        public async Task CreateRoleGranterMessage(CommandContext ctx,
            [Description("Заголовок")] string title,
            [Description("Описание")] string description,
            [Description("То, что написано перед ролями, по дефолту \"Привязка ролей\"")] string rolesFieldName = "Привязка ролей",
            string footer = "Для выдачи поставьте соответствующий смайлик")
        {
            await Core.Instance.RoleGranter.CreateRoleMessageAsync( ctx.Channel, title, description, rolesFieldName, footer);
        }

        [RequireRoles(RoleCheckMode.Any, "Programmer")]
        [Command("rgreset")]
        public async Task Reset(CommandContext ctx)
        {
            await Core.Instance.RoleGranter.Reset();
        }
        [Command("RGaddRole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Добавляет роль в список ролей для выдачи через сообщение, обновляет если сообщение уже существует, то необходимости пересоздавать можно воспользоваться командами изменения")]
        public async Task AddRoleFromEmoji(CommandContext ctx, DiscordRole role, DiscordEmoji emoji, string description)
        {
            await ctx.Message.DeleteAsync();
            bool succeed = await Core.Instance.RoleGranter.AddRoleByEmoji(role, emoji, description);
            if(!succeed)
            {
                var msg = await ctx.Channel.SendMessageAsync("Сообщение для выдачи ролей на задано или такая роль уже существует");
                await Task.Delay(TimeSpan.FromSeconds(3));
                await msg.DeleteAsync();
            }
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeTitle")]
        public async Task ChangeTitle(CommandContext ctx, string content)
        {
            await Core.Instance.RoleGranter.ChangeEmbedAsync(title: content);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeDescription")]
        public async Task ChangeDescription(CommandContext ctx, string content)
        {
            await Core.Instance.RoleGranter.ChangeEmbedAsync(description: content);
        }

        [Description("Меняет заголовок поля с ролями")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeRFName")]
        public async Task ChangeRFName(CommandContext ctx, string content)
        {
            await Core.Instance.RoleGranter.ChangeEmbedAsync(rfName: content);
        }

        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("RGchangeFooter")]
        public async Task ChangeFooter(CommandContext ctx, string content)
        {
            await Core.Instance.RoleGranter.ChangeEmbedAsync(footer: content);
        }


    }
}
