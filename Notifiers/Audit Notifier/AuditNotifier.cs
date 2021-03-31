
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Models;
using Volodya.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using LSSKeeper.Main;
using LSSKeeper.Commands;
using Volodya.Commands;
using DSharpPlus.CommandsNext;

namespace Volodya
{

    [JsonObject(MemberSerialization.OptIn)]
    public class AuditNotifier : KeeperModule
    {
        private DiscordChannel _auditChannel;
        private DiscordAuditLogEntry _newEntry;
        private DiscordAuditLogEntry _lastHandledEntry;

        private DiscordEmbedBuilder _entryBuilder = new DiscordEmbedBuilder();

        [JsonProperty]
        private ulong? _channelId;
        [JsonProperty]
        private Dictionary<string, string> phraseResponses = new Dictionary<string, string>();
        [JsonProperty]
        private List<string>  blacklistedWords = new List<string>();


        #region Module Methods
        public override async Task InitializeAsync(DiscordClient c, DiscordGuild guild) //Subscribe event methods to current guild methods //Need to modify ref
        {
            await base.InitializeAsync(c, guild);
            SubscribeToAllEvents();
        }
        public override void RegisterCommands(CommandsNextExtension commands)
        {
            AuditNotifierCommands.ANotifier = this;
            CommandsType = typeof(AuditNotifierCommands);
            base.RegisterCommands(commands);
        }

        protected override async Task InitializeConfigAsync()
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(ConfigNames.GUILDEVENTS);
                var json = JsonConvert.DeserializeObject<AuditNotifier>(jsonString);
                _channelId = json._channelId;
                if (_channelId != null)
                    _auditChannel = DefaultGuild.GetChannel((ulong)_channelId);
                phraseResponses = json.phraseResponses;

            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Не удалось инициализировать конфиг аудит лога " + e.Message);

            }
        }
        protected override async Task SaveAsync()
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(this);
                await File.WriteAllTextAsync(ConfigNames.GUILDEVENTS, jsonString);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Не удалось сохранить конфиг аудит лога " + e.Message);
            }
        }
        #endregion

        #region Commands
        public async Task SetChannelAsync(DiscordChannel channel)
        {
            _auditChannel = channel;
            _channelId = channel.Id;
            await SaveAsync();
        }
        public async Task<bool> TryBlacklistWordAsync(string p)
        {
            if (blacklistedWords.Contains(p)) return false;
            
            blacklistedWords.Add(p);
            await SaveAsync();
            return true;
        }
        

        public async Task<bool> TryDeblacklistWordAsync(string word)
        {
            if(blacklistedWords.Remove(word))
            {
                await SaveAsync();
                return true;
            }
            return false;
        }
        public List<string> GetBlacklistedWords()
        {
            return blacklistedWords;
        }
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        
        public async Task<bool> TryAddResponseAsync(string keyPhrase, string response)
        {
            if (phraseResponses.ContainsKey(keyPhrase)) return false;
            
            phraseResponses.Add(keyPhrase, response);
            await SaveAsync();
            return true;
        }
        public async Task<bool> TryRemoveResponseAsync(string keyPhrase)
        {
            if (phraseResponses.Remove(keyPhrase))
            {
                await SaveAsync();
                return true;
            }
            return false;

        }
        public string[] GetResponses()
        {
            return phraseResponses.Keys.ToArray();
        }
        #endregion
        #region Actions
        #region Guild Actions
        private async Task IntegrationsUpdated(DiscordClient sender, GuildIntegrationsUpdateEventArgs e)
        {
            var intsEntry = await GetNewEntryAsync() as DiscordAuditLogIntegrationEntry;
            if (intsEntry == null) return;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(intsEntry, "Обновление интеграций");
            _entryBuilder.SetDescription("Для более подробной информации обратитесь в журнал аудита");
            await SendMessageToAuditAsync(true, embed: _entryBuilder);

        }
        private async Task WebhooksUpdated(DiscordClient sender, WebhooksUpdateEventArgs e)
        {
            var webhookEntry = await GetNewEntryAsync() as DiscordAuditLogWebhookEntry;

            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(webhookEntry);
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
                        _entryBuilder.AddNamePropertyChange(webhookEntry.NameChange);
                        _entryBuilder.AddChannelPropertyChange("Канал", webhookEntry.ChannelChange);
                        _entryBuilder.AddPropertyChange("Аватар", webhookEntry.AvatarHashChange);
                    }
                    break;
                case (AuditLogActionType.WebhookDelete):
                    {
                        action = "Удаление";
                    }
                    break;

                default: return;
            }
            _entryBuilder.SetTitle(action + " вебхука");
            _entryBuilder.SetDescription($"{action} вебхука {webhookEntry.Target.Name}");

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }
        private async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
        {
            var guEntry = await GetNewEntryAsync() as DiscordAuditLogGuildEntry;
            if (guEntry == null) return; //Defense from something(xD)

            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(guEntry, "Обновление параметров сервера");

            _entryBuilder.AddNamePropertyChange(guEntry.NameChange);
            _entryBuilder.AddPropertyChange("Регион", guEntry.RegionChange);
            _entryBuilder.AddPropertyChange("Уровень фильтрации откровенного контента", guEntry.ExplicitContentFilterChange);
            _entryBuilder.AddPropertyChange("Требования к верификации", guEntry.VerificationLevelChange);
            _entryBuilder.AddPropertyChange("Аватар", guEntry.IconChange);
            _entryBuilder.AddPropertyChange("Стандартные настройки уведомлений", guEntry.NotificationSettingsChange);
            _entryBuilder.AddPropertyChange("Двухфакторная аутентификация", guEntry.MfaLevelChange);
            _entryBuilder.AddPropertyChange("Изображение при инвайте", guEntry.SplashChange);

            _entryBuilder.AddChannelPropertyChange("Афк", guEntry.AfkChannelChange);
            _entryBuilder.AddChannelPropertyChange("Системный", guEntry.SystemChannelChange);

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }
        #endregion
        #region Channel Actions
        private async Task ChannelPinsUpdated(DiscordClient sender, ChannelPinsUpdateEventArgs e)
        {
            var pinEntry = await GetNewEntryAsync() as DiscordAuditLogMessagePinEntry;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(pinEntry);
            var action = pinEntry.ActionType == AuditLogActionType.MessagePin ? "Закрепление" : "Открепление";
            _entryBuilder.SetTitle(action + " сообщения");
            _entryBuilder.SetDescription($"{action} сообщения в канале {pinEntry.Channel.Name}");
            var msg = await pinEntry.Channel.GetMessageAsync(pinEntry.Message.Id);

            _entryBuilder.AddMesage(msg);
            _entryBuilder.AddField("Прямая ссылка", msg.JumpLink.AbsoluteUri);

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }
        private async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            _newEntry = await GetNewEntryAsync();
            if (_newEntry.ActionType == AuditLogActionType.MemberUpdate)
            {
                var muEntry = _newEntry as DiscordAuditLogMemberUpdateEntry;
                _entryBuilder = EmbedBuilderExtensions.CreateForAudit(_newEntry);
                _entryBuilder.SetTitle("Обновление пользователя");
                _entryBuilder.AddPropertyChange("Мут", muEntry.MuteChange);
                _entryBuilder.AddPropertyChange("Заглушение", muEntry.DeafenChange);
            }
            else return;
            await SendMessageToAuditAsync(true, embed: _entryBuilder);
        }
        private async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
        {
            var ccEntry = await GetNewEntryAsync() as DiscordAuditLogChannelEntry;
            string commonType = ccEntry.Target.Type.ToRusCommon();
            string type = ccEntry.Target.Type == ChannelType.Category ? "" : $", тип канала: {ccEntry.Target.Type.ToRusString()}";
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(ccEntry, $"Создание {commonType}");
            _entryBuilder.SetDescription($"Создание {commonType} {ccEntry.NameChange.After} {type}");
            await SendMessageToAuditAsync(embed: _entryBuilder);

        }
        private async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
        {

            _newEntry = await GetNewEntryAsync();
            _entryBuilder = new DiscordEmbedBuilder();

            string commonType = e.ChannelAfter.Type.ToRusCommon();
            string channelName = e.ChannelAfter.Name;


            var entryType = _newEntry?.ActionType;
            bool checkForSameEntry = false;
            switch (entryType)
            {
                case (AuditLogActionType.ChannelUpdate):
                    {
                        checkForSameEntry = true;//Important because in second case we have defense by ow == null, and if any small updates happen don't wanna send same channel update entries
                        var cuEntry = _newEntry as DiscordAuditLogChannelEntry;
                        _entryBuilder = EmbedBuilderExtensions.CreateForAudit(cuEntry,
                            $"Обновление параметров {commonType}",
                            $"Обновлены параметры у {commonType} {channelName}");

                        _entryBuilder.AddNamePropertyChange(cuEntry.NameChange);

                        _entryBuilder.AddPropertyChange("Битрейт", cuEntry.BitrateChange);
                        _entryBuilder.AddPropertyChange("NSFW", cuEntry.NsfwChange);
                        _entryBuilder.AddPropertyChange("Слоумод", cuEntry.PerUserRateLimitChange);
                        _entryBuilder.AddPropertyChange("Тема", cuEntry.TopicChange);
                        break;
                    }
                default: // case if overwrite entry
                    {
                        var owsBefore = e.ChannelBefore.PermissionOverwrites;
                        var owsAfter = e.ChannelAfter.PermissionOverwrites;
                        OverwriteUpdateInformation owUpdInfo = new OverwriteUpdateInformation(owsBefore, owsAfter);
                        if (_newEntry != null)
                        {
                            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(_newEntry);
                            _entryBuilder.AddField("Изменённые оверврайты", string.Join("\n", owUpdInfo.Changes));
                        }

                        var ow = owUpdInfo.GetAffectedOverwrite();
                        if (ow == null) return; //If we don't have overwriteupdates at all

                        _entryBuilder.SetTitle($"{owUpdInfo.Action} оверврайтов");

                        string subj = ow.Type == OverwriteType.Role ?
                            "роли " + ow.GetRoleAsync().Result.Name :
                            "пользователя " + ow.GetMemberAsync().Result.Username;

                        _entryBuilder.SetDescription($"{owUpdInfo.Action} оверврайтов для {subj} у {e.ChannelAfter.Type.ToRusCommon()} {channelName} ");
                    }
                    break;
            }
            await SendMessageToAuditAsync(checkForSameEntry, embed: _entryBuilder); //Don't set true on defense from same entry,

        }
        private async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
        {
            _newEntry = await GetNewEntryAsync();
            var cdEntry = _newEntry as DiscordAuditLogChannelEntry;
            if (cdEntry == null) return; // Defense from cases when we delete category with channels under it
            var cType = cdEntry.Target.Type;

            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(cdEntry, "Удаление " + cType.ToRusCommon());
            var shortDesc = $"Удаление {cType.ToRusCommon()} {cdEntry.Target.Name}";
            var desc = cType == ChannelType.Category ? shortDesc : shortDesc + $", тип канала: {cType.ToRusString()}";
            _entryBuilder.SetDescription(desc);
            await SendMessageToAuditAsync(true, embed: _entryBuilder);
        }

        #endregion
        #region Role Actions
        private async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
        {
            var rcEntry = await GetNewEntryAsync() as DiscordAuditLogRoleUpdateEntry;

            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(rcEntry, "Создание роли", $"Создана роль {rcEntry.Target.Name}");

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }
        private async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
        {

            var roleUpdEntry = await GetNewEntryAsync() as DiscordAuditLogRoleUpdateEntry;
            if (roleUpdEntry == null) return;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(roleUpdEntry,
                "Обновление роли",
                $"Обновлена роль {roleUpdEntry.Target.Name}");
            _entryBuilder.AddNamePropertyChange(roleUpdEntry.NameChange);

            _entryBuilder.AddPropertyChange("Возможность упоминания", roleUpdEntry.MentionableChange);
            _entryBuilder.AddPropertyChange("Уникальность", roleUpdEntry.HoistChange);
            _entryBuilder.AddPropertyChange("Позиция", roleUpdEntry.PositionChange);

            if (roleUpdEntry.ColorChange != null)
                _entryBuilder.AddField("Измёнён цвет", roleUpdEntry.Target.Mention, true);

            if (roleUpdEntry.PermissionChange != null)
                _entryBuilder.AddField("Обновление привилегий", roleUpdEntry.PermissionChange.ToRusString());

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }


        private async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
        {

            var roleDelEntry = await GetNewEntryAsync() as DiscordAuditLogRoleUpdateEntry;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(roleDelEntry, "Удаление роли", $"Удалена роль {roleDelEntry.Target.Mention}");
            await SendMessageToAuditAsync(embed: _entryBuilder);
        }

        #endregion
        #region Ban Actions
        private async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
        {
            var banEntry = await GetNewEntryAsync() as DiscordAuditLogBanEntry;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(banEntry, "Бан", $"Пользователь {banEntry.Target.DisplayName} был забанен");

            var reason = banEntry.Reason.IsRelevant() ? banEntry.Reason : "Не указана";
            _entryBuilder.AddField("Причина", reason);

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }

        private async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
        {
            var unbanEntry = await GetNewEntryAsync() as DiscordAuditLogBanEntry;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(unbanEntry, "Разбан", $"Пользователь {unbanEntry.Target.Username} был разбанен");
            await SendMessageToAuditAsync(embed: _entryBuilder);
        }

        #endregion
        #region Message Actions
        private async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            var msgContent = e.Message.Content;
            if (blacklistedWords.Any((x) => msgContent.Contains(x)))
            {
                await e.Message.DeleteAsync();
                await e.Channel.SendTempMessageAsync($"Так говорить нельзя, фу таким быть {e.Author.Mention}");
                return;
            }

            foreach (var pr in phraseResponses)
            {
                if (e.Message.Content.Contains(pr.Key))
                {
                    await e.Channel.SendMessageAsync(pr.Value);
                    return;
                }
            }
        }
        private async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Author.IsBot) return;
            _entryBuilder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = e.Author.Username, IconUrl = e.Author.AvatarUrl },
                Title = $"Сообщение отредактировано в канале {e.Message.Channel.Name}"
            };

            if (e.MessageBefore != null && e.MessageBefore.Content.Equals(e.Message.Content, StringComparison.OrdinalIgnoreCase)) return;

            string oldContent = e.MessageBefore != null && e.MessageBefore.Content.IsRelevant() ?
                e.MessageBefore.Content :
                "Информация о старом содержании некэширована";

            _entryBuilder.AddBeforeAfter("Содержание", oldContent, e.Message.Content);
            _entryBuilder.AddField("Прямая ссылка", e.Message.JumpLink.AbsoluteUri);
            await SendMessageToAuditAsync(embed: _entryBuilder);
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
            DiscordEmbedBuilder entryBuilder = new DiscordEmbedBuilder();
            if (isSelfDelete)
            {
                if (msg.Author.IsBot) return;
                entryBuilder.SetAuthor(msg.Author);
                entryBuilder.WithFooter($"Время действия {DateTime.Now.ToLongDateString()}");
            }
            else
            {
                if (mdEntry.UserResponsible.IsBot) return;
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
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(invCreateEntry, "Создание приглашения");
            var invite = e.Invite;
            _entryBuilder.SetDescription("Создание приглашения " + invite.Code);
            if (invite.Channel != null)
                _entryBuilder.AddField("Предназначен для: ", invite.Channel.Name);

            _entryBuilder.AddField("Время истечения", (invite.MaxAge/3600).ToString() + 'ч');

            _entryBuilder.AddField("Максимальное количество использований", invite.MaxUses.ToString());

            _entryBuilder.AddField("Членство только на время приглашения", invite.IsTemporary.ToString());

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }
        private async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
        {
            var invDelEntry = await GetNewEntryAsync() as DiscordAuditLogInviteEntry;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(invDelEntry, "Удаление приглашения");
            var invite = e.Invite;
            _entryBuilder.SetDescription("Удаление приглашения " + invite.Code);
            _entryBuilder.AddField("Количество использований", invite.Uses.ToString());

            await SendMessageToAuditAsync(embed: _entryBuilder);
        }
        #endregion
        #region Member Actions
        private async Task GuildMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)   
        {
            var name = e.Member.Username;
            if (HasBlacklistedWord(name))
            {
                await _auditChannel.SendMessageAsync("Пользователь в теории забанен" + e.Member.Mention);

                //await e.Member.BanAsync(reason: "Запрещённое слово в никнейме");
                return;
            }
            
            var userDM = await e.Member.CreateDmChannelAsync();
            await userDM?.SendMessageAsync("Добро пожаловать в ЛСС!");
        }


       
        private async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
        {

            var member = await DefaultGuild.GetMemberAsync(e.UserAfter.Id);
            if (member == null) return;
            _entryBuilder = new DiscordEmbedBuilder();
            _entryBuilder.SetAuthor(e.UserAfter);
            _entryBuilder.SetTitle("Обновление параметров пользователя");

            if (e.UserBefore.Username != e.UserAfter.Username)
            {
                if (HasBlacklistedWord(e.UserAfter.Username))
                {
                    await _auditChannel.SendMessageAsync("Пользователь в теории забанен" + member.Mention);
                    //await member.BanAsync(reason: "Запрещённое слово в никнейме");
                    return;
                }
                _entryBuilder.AddBeforeAfter("Имя", e.UserBefore.Username, e.UserAfter.Username);
            }
            if (e.UserBefore.Discriminator != e.UserAfter.Discriminator)
            {
                _entryBuilder.AddBeforeAfter("Дискриминатор", e.UserBefore.Discriminator, e.UserAfter.Discriminator);
            }
            if (e.UserBefore.AvatarUrl != null && e.UserBefore.AvatarUrl != e.UserAfter.AvatarUrl)
            {
                _entryBuilder.SetTitle("Пользователь обновил аватар");
                try
                {
                    _entryBuilder.WithImageUrl(e.UserAfter.AvatarUrl);
                }
                catch
                {
                    _entryBuilder.SetDescription("Не удалось установить ссылку на изображение");
                }
            }
            await SendMessageToAuditAsync(embed: _entryBuilder);

        }
        private async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            var muEntry = await GetNewEntryAsync() as DiscordAuditLogMemberUpdateEntry;
            if (muEntry == null) return;
            _entryBuilder = EmbedBuilderExtensions.CreateForAudit(muEntry, $"Изменение пользователя {muEntry.Target.Username}");
            if (muEntry.UserResponsible.IsBot) return;
            if(HasBlacklistedWord(muEntry.NicknameChange?.After))
            {
                await muEntry.Target.ModifyAsync((x) => x.Nickname = muEntry.Target.Username);
                return;
            }
            _entryBuilder.AddNamePropertyChange(muEntry.NicknameChange);
            _entryBuilder.AddRoles("Добавленные", muEntry.AddedRoles);
            _entryBuilder.AddRoles("Удалённые", muEntry.RemovedRoles);
            await SendMessageToAuditAsync(true, embed: _entryBuilder);

        }
        private async Task GuildMemberRemoved(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            var kickEntry = await GetNewEntryAsync() as DiscordAuditLogKickEntry;
            
            if (kickEntry != null)
            {
                _entryBuilder = EmbedBuilderExtensions.CreateForAudit(kickEntry, "Кик", $"Пользователь {kickEntry.Target.Username} был кикнут");
                var reason = kickEntry.Reason.IsRelevant() ? kickEntry.Reason : "Не указана";
                _entryBuilder.AddField("Причина", reason);
                await SendMessageToAuditAsync(embed: _entryBuilder);
            }
            else
            {
                var banEntry = await GetNewEntryAsync() as DiscordAuditLogBanEntry;
                if (banEntry != null) return;
                _entryBuilder = new DiscordEmbedBuilder();
                _entryBuilder.SetAuthor(e.Member);
                _entryBuilder.SetTitle("Пользователь покинул нас");
                _entryBuilder.SetDescription($"{e.Member.Mention} joined {e.Member.JoinedAt.LocalDateTime}");
                await SendMessageToAuditAsync(content: $"Пользователь {e.Member.Mention} покинул нас");
            }

        }
        #endregion
        #endregion
        #region Private Methods
        private void SubscribeToAllEvents()
        {
            Client.GuildMemberAdded += GuildMemberAdded;
            Client.GuildMemberUpdated += GuildMemberUpdated;
            Client.GuildMemberRemoved += GuildMemberRemoved;

            Client.GuildBanAdded += GuildBanAdded;
            Client.GuildBanRemoved += GuildBanRemoved;
            Client.ChannelPinsUpdated += ChannelPinsUpdated;
            Client.VoiceStateUpdated += VoiceStateUpdated;

            Client.MessageCreated += MessageCreated;
            Client.MessageUpdated += MessageUpdated;
            Client.MessageDeleted += MessageDeleted;

            Client.GuildRoleCreated += GuildRoleCreated;
            Client.GuildRoleUpdated += GuildRoleUpdated;
            Client.GuildRoleDeleted += GuildRoleDeleted;

            Client.UserUpdated += UserUpdated;
            Client.ChannelCreated += ChannelCreated;
            Client.ChannelUpdated += ChannelUpdated;
            Client.ChannelDeleted += ChannelDeleted;

            Client.GuildUpdated += GuildUpdated;
            Client.WebhooksUpdated += WebhooksUpdated;
            Client.GuildIntegrationsUpdated += IntegrationsUpdated;

            Client.InviteCreated += InviteCreated;
            Client.InviteDeleted += InviteDeleted;
        }

        private bool HasBlacklistedWord(string name)
        {
            return blacklistedWords.Any((x) => name.Contains(x));
        }
        private async Task<DiscordAuditLogEntry> GetNewEntryAsync()
        {
            try
            {
                var audit = await DefaultGuild.GetAuditLogsAsync(1);
                _newEntry = audit.First();
                return _newEntry;
            }
            catch
            {
                _newEntry = null;
                return _newEntry;
            }
        }

        private async Task SendMessageToAuditAsync(bool checkForSameEntry = false, string content = null, DiscordEmbedBuilder embed = null)
        {
            if (_auditChannel == null || (checkForSameEntry && _newEntry.Id == _lastHandledEntry?.Id)) return;
            _lastHandledEntry = _newEntry;
            await _auditChannel.SendMessageAsync(embed: embed, content: content);
        }

        #endregion
    }
}
