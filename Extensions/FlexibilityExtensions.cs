using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper.Extensions
{
    public static class FlexibilityExtensions
    {
        public static bool IsRelevant<T>(this IReadOnlyList<T> list)
        {
            return list != null && list.Count > 0;
        }
        public static bool IsRelevant(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
    }
}
