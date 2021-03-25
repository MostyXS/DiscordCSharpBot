using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volodya.Extensions
{
    public static class FlexibilityExtensions
    {

        public static string ToRusAge(this int integer)
        {
            if(integer % 100 >= 5 && integer % 100 <= 20 || integer % 10 >= 5)
            {
                return $"{integer} лет";
            }
            else if(integer % 10 == 1)
            {
                return $"{integer} год";
            }
            else if(integer % 10 < 5)
            {
                return $"{integer} года";
            }
            else
            {
                return $"{integer}";
            }
            
        }
        public static bool IsRelevant<T>(this IReadOnlyList<T> list)
        {
            return list != null && list.Count > 0;
        }
        public static bool IsRelevant(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// Отправляет сообщение и удаляет через 5 секунд, только для сообщений с контентом
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task SendTempMessageAsync(this DiscordChannel c, string content)
        {
            var msg = await c.SendMessageAsync(content);
            await Task.Delay(TimeSpan.FromSeconds(5));
            await msg.DeleteAsync();
        }
        public static DiscordEmoji ToEmoji(this string s, BaseDiscordClient c)
        {
            try
            {
                return DiscordEmoji.FromName(c, s);
            }
            catch
            {
                return DiscordEmoji.FromUnicode(c, s);
            }
        }
    }
}
