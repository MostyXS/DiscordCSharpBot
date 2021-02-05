
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using LSSKeeper.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper
{


    public class GuildEvents
    {
        public DiscordChannel AuditChannel { private get; set; }
        DiscordGuild defaultGuild;

        DiscordAuditLogEntry newEntry;//Any entry
        DiscordAuditLogEntry lastHandledEntry; //new entry that comes to messageSending

        DiscordEmbedBuilder entryBuilder = new DiscordEmbedBuilder();

        public GuildEvents( DiscordChannel auditChannel, DiscordGuild defaultGuild)
        {
            this.AuditChannel = auditChannel;
            this.defaultGuild = defaultGuild;
        }
        public void SubscribeToGuildEvents(DiscordClient c) //Subscribe event methods to current guild methods //Need to modify ref
        {

            c.GuildMemberAdded += GuildMemberAdded; //Done 100% //Not with audit log
            c.GuildMemberUpdated += GuildMemberUpdated; //Done 100%
            c.GuildMemberRemoved += GuildMemberRemoved; //Done 100%

            c.GuildBanAdded += GuildBanAdded;//Done 100%
            c.GuildBanRemoved += GuildBanRemoved; //Done 100%

            c.ChannelPinsUpdated += ChannelPinsUpdated; //Done 100%
            c.VoiceStateUpdated += VoiceStateUpdated; //Done 100%
            c.MessageUpdated += MessageUpdated;//Done 100% //Not with audit log
            c.MessageDeleted += MessageDeleted; //Done 100%

            c.GuildRoleCreated += GuildRoleCreated; //Done 100%
            c.GuildRoleUpdated += GuildRoleUpdated; //Done 100%
            c.GuildRoleDeleted += GuildRoleDeleted; //Done 100%

            c.ChannelCreated += ChannelCreated; //Done 100%
            c.ChannelUpdated += ChannelUpdated; //Done 100%
            c.ChannelDeleted += ChannelDeleted; //Done 100%

            c.GuildUpdated += GuildUpdated; //Done 100%
            c.WebhooksUpdated += WebhooksUpdated; //Done 100%
            c.GuildIntegrationsUpdated += IntegrationsUpdated; //Done 100% probably

            c.InviteCreated += InviteCreated; //Done 100% //Should implement invite system
            c.InviteDeleted += InviteDeleted; //Done 100%

        }

        #region Guild Actions
        private async Task IntegrationsUpdated(DiscordClient sender, GuildIntegrationsUpdateEventArgs e)
        {
            var intsEntry = await GetNewEntryAsync() as DiscordAuditLogIntegrationEntry;
            if (intsEntry == null) return;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(intsEntry, "Обновление интеграций");
            entryBuilder.SetDescription("Для более подробной информации обратитесь в журнал аудита");
            await SendMessageToAuditAsync(true, embed: entryBuilder);

        }
        private async Task WebhooksUpdated(DiscordClient sender, WebhooksUpdateEventArgs e)
        {
            var webhookEntry = await GetNewEntryAsync() as DiscordAuditLogWebhookEntry;

            entryBuilder = EmbedBuilderExtensions.CreateForAudit(webhookEntry);
            string action;
            switch (webhookEntry.ActionType)
            {
                case (AuditLogActionType.WebhookCreate):
                    {
                        action = "Создание";
                    }
                    break;
                case (AuditLogActionType.WebhookUpdate):
                    {
                        action = "Обновление";
                        entryBuilder.AddNamePropertyChange(webhookEntry.NameChange);
                        entryBuilder.AddChannelPropertyChange("Канал", webhookEntry.ChannelChange);
                        entryBuilder.AddPropertyChange("Аватар", webhookEntry.AvatarHashChange);
                    }
                    break;
                case (AuditLogActionType.WebhookDelete):
                    {
                        action = "Удаление";
                    }
                    break;

                default: return;
            }
            entryBuilder.SetTitle(action + " вебхука");
            entryBuilder.SetDescription($"{action} вебхука {webhookEntry.Target.Name}");

            await SendMessageToAuditAsync(embed: entryBuilder);
        }
        private async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
        {
            var guEntry = await GetNewEntryAsync() as DiscordAuditLogGuildEntry;
            if (guEntry == null) return; //Defense from something(xD)

            entryBuilder = EmbedBuilderExtensions.CreateForAudit(guEntry, "Обновление параметров сервера");

            entryBuilder.AddNamePropertyChange(guEntry.NameChange);
            entryBuilder.AddPropertyChange("Регион", guEntry.RegionChange);
            entryBuilder.AddPropertyChange("Уровень фильтрации откровенного контента", guEntry.ExplicitContentFilterChange);
            entryBuilder.AddPropertyChange("Требования к верификации", guEntry.VerificationLevelChange);
            entryBuilder.AddPropertyChange("Аватар", guEntry.IconChange);
            entryBuilder.AddPropertyChange("Стандартные настройки уведомлений", guEntry.NotificationSettingsChange);
            entryBuilder.AddPropertyChange("Двухфакторная аутентификация", guEntry.MfaLevelChange);
            entryBuilder.AddPropertyChange("Изображение при инвайте", guEntry.SplashChange);

            entryBuilder.AddChannelPropertyChange("Афк", guEntry.AfkChannelChange);
            entryBuilder.AddChannelPropertyChange("Системный", guEntry.SystemChannelChange);

            await SendMessageToAuditAsync(embed: entryBuilder);
        }
        #endregion
        #region Channel Actions
        private async Task ChannelPinsUpdated(DiscordClient sender, ChannelPinsUpdateEventArgs e)
        {
            var pinEntry = await GetNewEntryAsync() as DiscordAuditLogMessagePinEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(pinEntry);
            var action = pinEntry.ActionType == AuditLogActionType.MessagePin ? "Закрепление" : "Открепление";
            entryBuilder.SetTitle(action + " сообщения");
            entryBuilder.SetDescription($"{action} сообщения в канале {pinEntry.Channel.Name}");
            var msg = await pinEntry.Channel.GetMessageAsync(pinEntry.Message.Id);

            entryBuilder.AddMesage(msg);
            entryBuilder.AddField("Прямая ссылка", msg.JumpLink.AbsoluteUri);

            await SendMessageToAuditAsync(embed: entryBuilder);
        }
        private async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            newEntry = await GetNewEntryAsync();
            if (newEntry.ActionType == AuditLogActionType.MemberUpdate)
            {
                var muEntry = newEntry as DiscordAuditLogMemberUpdateEntry;
                entryBuilder = EmbedBuilderExtensions.CreateForAudit(newEntry);
                entryBuilder.SetTitle("Обновление пользователя");
                entryBuilder.AddPropertyChange("Мут", muEntry.MuteChange);
                entryBuilder.AddPropertyChange("Заглушение", muEntry.DeafenChange);
            }
            else return;
            await SendMessageToAuditAsync(true, embed: entryBuilder);
        }
        private async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
        {
            var ccEntry = await GetNewEntryAsync() as DiscordAuditLogChannelEntry;
            string commonType = ccEntry.Target.Type.ToRusCommon();
            string type = ccEntry.Target.Type == ChannelType.Category ? "" : $", тип канала: {ccEntry.Target.Type.ToRusString()}";
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(ccEntry, $"Создание {commonType}");
            entryBuilder.SetDescription($"Создание {commonType} {ccEntry.NameChange.After} {type}");
            await SendMessageToAuditAsync(embed: entryBuilder);

        }
        private async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
        {

            newEntry = await GetNewEntryAsync();
            entryBuilder = new DiscordEmbedBuilder();

            string commonType = e.ChannelAfter.Type.ToRusCommon();
            string channelName = e.ChannelAfter.Name;


            var entryType = newEntry?.ActionType;
            bool checkForSameEntry = false;
            switch (entryType)
            {
                case (AuditLogActionType.ChannelUpdate):
                    {
                        checkForSameEntry = true;//Important because in second case we have defense by ow == null, and if any small updates happen don't wanna send same channel update entries
                        var cuEntry = newEntry as DiscordAuditLogChannelEntry;
                        entryBuilder = EmbedBuilderExtensions.CreateForAudit(cuEntry,
                            $"Обновление параметров {commonType}",
                            $"Обновлены параметры у {commonType} {channelName}");

                        entryBuilder.AddNamePropertyChange(cuEntry.NameChange);

                        entryBuilder.AddPropertyChange("Битрейт", cuEntry.BitrateChange);
                        entryBuilder.AddPropertyChange("NSFW", cuEntry.NsfwChange);
                        entryBuilder.AddPropertyChange("Слоумод", cuEntry.PerUserRateLimitChange);
                        entryBuilder.AddPropertyChange("Тема", cuEntry.TopicChange);
                        break;
                    }
                default: // case if overwrite entry
                    {
                        var owsBefore = e.ChannelBefore.PermissionOverwrites;
                        var owsAfter = e.ChannelAfter.PermissionOverwrites;
                        OverwriteUpdateInformation owUpdInfo = new OverwriteUpdateInformation(owsBefore, owsAfter);
                        if (newEntry != null)
                        {
                            entryBuilder = EmbedBuilderExtensions.CreateForAudit(newEntry);
                            entryBuilder.AddField("Изменённые оверврайты", string.Join("\n", owUpdInfo.Changes));
                        }

                        var ow = owUpdInfo.GetAffectedOverwrite();
                        if (ow == null) return; //If we don't have overwriteupdates at all

                        entryBuilder.SetTitle($"{owUpdInfo.Action} оверврайтов");

                        string subj = ow.Type == OverwriteType.Role ?
                            "роли " + ow.GetRoleAsync().Result.Name :
                            "пользователя " + ow.GetMemberAsync().Result.Username;

                        entryBuilder.SetDescription($"{owUpdInfo.Action} оверврайтов для {subj} у {e.ChannelAfter.Type.ToRusCommon()} {channelName} ");
                    }
                    break;
            }
            await SendMessageToAuditAsync(checkForSameEntry, embed: entryBuilder); //Don't set true on defense from same entry,

        }
        private async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
        {
            newEntry = await GetNewEntryAsync();
            var cdEntry = newEntry as DiscordAuditLogChannelEntry;
            if (cdEntry == null) return; // Defense from cases when we delete category with channels under it
            var cType = cdEntry.Target.Type;

            entryBuilder = EmbedBuilderExtensions.CreateForAudit(cdEntry, "Удаление " + cType.ToRusCommon());
            var shortDesc = $"Удаление {cType.ToRusCommon()} {cdEntry.Target.Name}";
            var desc = cType == ChannelType.Category ? shortDesc : shortDesc + $", тип канала: {cType.ToRusString()}";
            entryBuilder.SetDescription(desc);
            await SendMessageToAuditAsync(true, embed: entryBuilder);
        }

        #endregion
        #region Role Actions
        private async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
        {
            var rcEntry = await GetNewEntryAsync() as DiscordAuditLogRoleUpdateEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(rcEntry, "Создание роли", $"Создана роль {rcEntry.Target.Name}");

            await SendMessageToAuditAsync(embed: entryBuilder);
        }
        private async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
        {

            var roleUpdEntry = await GetNewEntryAsync() as DiscordAuditLogRoleUpdateEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(roleUpdEntry,
                "Обновление роли",
                $"Обновлена роль {roleUpdEntry.Target.Name}");
            entryBuilder.AddNamePropertyChange(roleUpdEntry.NameChange);

            entryBuilder.AddPropertyChange("Возможность упоминания", roleUpdEntry.MentionableChange);
            entryBuilder.AddPropertyChange("Уникальность", roleUpdEntry.HoistChange);
            entryBuilder.AddPropertyChange("Позиция", roleUpdEntry.PositionChange);

            if (roleUpdEntry.ColorChange != null)
                entryBuilder.AddField("Измёнён цвет", roleUpdEntry.Target.Mention, true);

            if (roleUpdEntry.PermissionChange != null)
                entryBuilder.AddField("Обновление привилегий", roleUpdEntry.PermissionChange.ToRusString());

            await SendMessageToAuditAsync(embed: entryBuilder);
        }


        private async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
        {

            var roleDelEntry = await GetNewEntryAsync() as DiscordAuditLogRoleUpdateEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(roleDelEntry, "Удаление роли", $"Удалена роль {roleDelEntry.Target.Mention}");
            await SendMessageToAuditAsync(embed: entryBuilder);
        }



        #endregion
        #region Ban Actions
        private async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
        {
            var banEntry = await GetNewEntryAsync() as DiscordAuditLogBanEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(banEntry, "Бан", $"Пользователь {banEntry.Target.DisplayName} был забанен");

            var reason = banEntry.Reason.IsRelevant() ? banEntry.Reason : "Не указана";
            entryBuilder.AddField("Причина", reason);

            await SendMessageToAuditAsync(embed: entryBuilder);
        }

        private async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
        {
            var unbanEntry = await GetNewEntryAsync() as DiscordAuditLogBanEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(unbanEntry, "Разбан", $"Пользователь {unbanEntry.Target.Username} был разбанен");
            await SendMessageToAuditAsync(embed: entryBuilder);
        }

        #endregion
        #region Message Actions
        private async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Author.IsBot) return;
            entryBuilder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = e.Author.Username, IconUrl = e.Author.AvatarUrl },
                Title = $"Сообщение отредактировано в канале {e.Message.Channel.Name}"
            };

            string oldContent = e.MessageBefore != null && e.MessageBefore.Content.IsRelevant() ?
                e.MessageBefore.Content :
                "Информация о старом содержании некэширована";
            entryBuilder.AddBeforeAfter("Содержание", oldContent, e.Message.Content);
            entryBuilder.AddField("Прямая ссылка", e.Message.JumpLink.AbsoluteUri);
            await SendMessageToAuditAsync(embed: entryBuilder);
        }

        private async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
        {
            
            var mdEntry = await GetNewEntryAsync() as DiscordAuditLogMessageEntry;
            var msg = e.Message;
            bool isSelfDelete = mdEntry == null;
            if (msg?.Author == null) //Происходит при перезапуске бота или если сообщение устарело //msg !=null is not working properly
            {
                
                string user = isSelfDelete ? "" : ", Удаливший: " + mdEntry.UserResponsible.Mention;
                await SendMessageToAuditAsync(content: $"Некэшированое сообщение удалено в канале " + e.Channel.Name + user);
                return;
            }
            if (msg.Author.IsBot) return;
            DiscordEmbedBuilder entryBuilder = new DiscordEmbedBuilder();
            if (isSelfDelete)
            {
                entryBuilder.SetAuthor(msg.Author);
            }
            else
            {
                entryBuilder = EmbedBuilderExtensions.CreateForAudit(mdEntry);
            }
            entryBuilder.SetTitle("Удаление сообщения");
            entryBuilder.SetDescription("Сообщение удалено в канале " + e.Channel.Name);
            entryBuilder.AddMesage(msg);
            await SendMessageToAuditAsync(embed: entryBuilder);


        }

        #endregion
        #region Invite Actions
        private async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
        {
            var invCreateEntry = await GetNewEntryAsync() as DiscordAuditLogInviteEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(invCreateEntry, "Создание приглашения");
            var invite = e.Invite;
            entryBuilder.SetDescription("Создание приглашения " + invite.Code);
            if (invite.Channel != null)
                entryBuilder.AddField("Предназначен для: ", invite.Channel.Name);

            entryBuilder.AddField("Время истечения", invite.MaxAge.ToString());

            entryBuilder.AddField("Максимальное количество использований", invite.MaxUses.ToString());

            entryBuilder.AddField("Членство только на время приглашения", invite.IsTemporary.ToString());

            await SendMessageToAuditAsync(embed: entryBuilder);
        }
        private async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
        {
            var invDelEntry = await GetNewEntryAsync() as DiscordAuditLogInviteEntry;
            entryBuilder = EmbedBuilderExtensions.CreateForAudit(invDelEntry, "Удаление приглашения");
            var invite = e.Invite;
            entryBuilder.SetDescription("Удаление приглашения " + invite.Code);
            entryBuilder.AddField("Количество использований", invite.Uses.ToString());

            await SendMessageToAuditAsync(embed: entryBuilder);
        }
        #endregion
        #region Member Actions
        private async Task GuildMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            var userDM = await e.Member.CreateDmChannelAsync();
            await userDM?.SendMessageAsync("Добро пожаловать в ЛСС!");
        }

        private async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            Console.WriteLine("Member");
            var muEntry = await GetNewEntryAsync() as DiscordAuditLogMemberUpdateEntry;
            if (muEntry == null) return;

            entryBuilder = EmbedBuilderExtensions.CreateForAudit(muEntry, $"Изменение члена гильдии {muEntry.Target.Username}");

            entryBuilder.AddNamePropertyChange(muEntry.NicknameChange);

            entryBuilder.AddRoles("Добавленные", muEntry.AddedRoles);
            entryBuilder.AddRoles("Удалённые", muEntry.RemovedRoles);

            await SendMessageToAuditAsync(true, embed: entryBuilder);
        }

        private async Task GuildMemberRemoved(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            var kickEntry = await GetNewEntryAsync() as DiscordAuditLogKickEntry;
            if (kickEntry != null)
            {
                entryBuilder = EmbedBuilderExtensions.CreateForAudit(kickEntry, "Кик", $"Пользователь {kickEntry.Target.Username} был кикнут");
                var reason = kickEntry.Reason.IsRelevant() ? kickEntry.Reason : "Не указана";
                entryBuilder.AddField("Причина", reason);
                await SendMessageToAuditAsync(embed: entryBuilder);
            }
            else
            {
                await SendMessageToAuditAsync(content: $"Пользователь {e.Member.Mention} покинул нас");
            }
        }
        #endregion
        #region Private Methods
        private async Task<DiscordAuditLogEntry> GetNewEntryAsync()
        {
            try
            {
                var audit = await defaultGuild.GetAuditLogsAsync(1);
                newEntry = audit.First();
                return newEntry;
            }
            catch
            {
                newEntry = null;
                return newEntry; //Case if we have overwrite create or overwrite delete
            }
        }

        private async Task SendMessageToAuditAsync(bool checkForSameEntry = false, string content = null, DiscordEmbedBuilder embed = null)
        {
            if (AuditChannel == null || (checkForSameEntry && newEntry.Id == lastHandledEntry?.Id)) return;
            lastHandledEntry = newEntry;
            await AuditChannel.SendMessageAsync(embed: embed, content: content);
        }

        #endregion
    }
}
