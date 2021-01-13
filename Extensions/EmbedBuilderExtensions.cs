using DSharpPlus.Entities;
using DSharpPlus.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOSCKeeper.Extensions
{
    public static class EmbedBuilderExtensions
    {
        public static void AddBeforeAfter(this DiscordEmbedBuilder builder, string name, string before, string after)
        {
            builder.AddField(name, $"До: {before}\n После: {after}");
        }
        public static void AddChannelPropertyChange(this DiscordEmbedBuilder builder, string cName, PropertyChange<DiscordChannel> change)
        {
            if(change !=null)
            {
                var before = change.Before.Name.IsRelevant() ? change.Before.Name : "Отсутствует";
                var after = change.After.Name.IsRelevant() ? change.After.Name : "Отсутствует";
                builder.AddBeforeAfter(cName +"Канал" , before, after);
                
            }
            
        }
        public static void AddMesage(this DiscordEmbedBuilder builder, DiscordMessage msg)
        {

            if(msg.Content.IsRelevant())
            {
                builder.AddField("Содержание:", msg.Content);
            }
            if (msg.Attachments.IsRelevant())
            {
                builder.AddField("Приложения:", string.Join(',', msg.Attachments.ToAttachmentsString()));
            }
            if(msg.Embeds.IsRelevant())
            {
                var firstEmbed = msg.Embeds.First();
                /*DiscordEmbedField firstField = null;
                if(firstEmbed.Fields.IsRelevant())
                {
                    firstField = firstEmbed.Fields[0];
                }
                string ffContent = firstField != null ? $"Первое поле\nИмя: {firstField.Name}\nСодержание: {firstField.Value}" : "";*/
                builder.AddField("Содержание первого вложения:", $"Заголовок: {firstEmbed.Title}\nОписание: {firstEmbed.Description}");
            }
            
        }
        public static void SetAuthor(this DiscordEmbedBuilder builder, DiscordUser user)
        {
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                IconUrl = user.AvatarUrl,
                Name = user.Mention
            };
        }
        public static void SetDescription(this DiscordEmbedBuilder builder , string desc)
        {
            builder.Description = desc;
        }
        public static void SetTitle(this DiscordEmbedBuilder builder, string title)
        {
            builder.Title = title;
        }
        public static DiscordEmbedBuilder CreateForAudit(DiscordAuditLogEntry entry, string title = null, string description = null, DiscordColor color = default)
        {
            return new DiscordEmbedBuilder
            {
                Author = AuditAuthor(entry),
                Title = title,
                Description = description,
                Footer = AuditFooter(entry),
                Color = color
            };
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="embedBuilder"></param>
        /// <param name="title"> title+"Роли"</param>
        /// <param name="roles"></param>
        public static void AddRoles(this DiscordEmbedBuilder embedBuilder,string title, IReadOnlyList<DiscordRole> roles)
        {
            if(roles.IsRelevant())
            {
                embedBuilder.AddField(title+" роли:", string.Join(',', roles.ToRolesString()));
            }
        }
        #region PropertyChange
        
        public static void AddPropertyChange<T>(this DiscordEmbedBuilder builder, string name, PropertyChange<T> change )
        {
            if (change != null)
            {
                var before = change.Before != null ? change.Before.ToString() : "";
                var after = change.After != null ? change.After.ToString() : "";
                builder.AddBeforeAfter(name, before, after);

            }
        }
        public static void AddNamePropertyChange(this DiscordEmbedBuilder builder, PropertyChange<string> nChange)
        {   if(nChange !=null)
            {
                var before = nChange.Before.IsRelevant() ? nChange.Before : "Стандартное имя";
                var after = nChange.After.IsRelevant() ? nChange.After : "Стандартное имя";
                builder.AddBeforeAfter("Имя", before, after);
            }
        }
        #endregion

        #region Private methods
        private static DiscordEmbedBuilder.EmbedFooter AuditFooter(DiscordAuditLogEntry newEntry)
        {
            return new DiscordEmbedBuilder.EmbedFooter { Text = $"ID: {newEntry.Id}  Время действия: {newEntry.CreationTimestamp.LocalDateTime}" };
        }
        private static DiscordEmbedBuilder.EmbedAuthor AuditAuthor(DiscordAuditLogEntry entry)
        {
            return new DiscordEmbedBuilder.EmbedAuthor()
            {
                IconUrl = entry.UserResponsible.AvatarUrl,
                Name = entry.UserResponsible.Username
            };
        }
        #endregion

    }
}
