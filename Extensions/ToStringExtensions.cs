using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Extensions
{
    public static class ToStringExtensions
    {
        public static string ToRusString(this ChannelType type)
        {
            switch(type)
            {
                case (ChannelType.Text):
                    return "текстовый";
                case (ChannelType.Voice):
                    return "голосовой";
                default:
                    return "неизвестный";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Категории if category, otherwise Канале</returns>
        public static string ToRusCommon(this ChannelType type)
        {
            switch(type)
            {
                case (ChannelType.Category):
                    return "категории";
                default:
                    return "канала";
                    
            }
        }
        public static string ToRusString(this PropertyChange<Permissions?> permissionChanges)
        {

            var permsBefore = Array.ConvertAll(permissionChanges.Before.Value.ToPermissionString().Split(','), db => db.Trim());
            var permsAfter = Array.ConvertAll(permissionChanges.After.Value.ToPermissionString().Split(','), db => db.Trim());
            string removed = string.Join(", ", permsBefore.Where((x) => !permsAfter.Contains(x)));
            var added = string.Join(", ", permsAfter.Where((x) => !permsBefore.Contains(x)));

            StringBuilder result = new StringBuilder();
            result.Append("Удалённые: ");
            result.Append(removed + '\n');
            result.Append("Добавленные: ");
            result.Append(added + '\n');
            return result.ToString();

        }
        public static List<string> ToRolesString(this IReadOnlyList<DiscordRole> roles)
        {
            List<string> result = new List<string>();
            foreach(var role in roles)
            {
                result.Add(role.Mention);
            }
            return result;
        }

        public static List<string> ToAttachmentsString(this IReadOnlyList<DiscordAttachment> attachments)
        {
            List<string> result = new List<string>();

            foreach(var a in attachments)
            {
                result.Add(a.FileName);
                if(a.Url !=null)
                {
                    result.Add(a.Url);
                }
            }
            return result;
        }
        

    }
}












