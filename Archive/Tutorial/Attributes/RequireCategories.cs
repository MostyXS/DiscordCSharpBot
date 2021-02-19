using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Valera.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Valera.Commands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    class RequireCategories : CheckBaseAttribute
    {
        public IReadOnlyList<string> CategoryNames { get; }
        public ChannelCheckMode CheckMode { get; }

        public RequireCategories(ChannelCheckMode checkMode, params string[] channelNames)
        {
            CheckMode = checkMode;
            CategoryNames = new ReadOnlyCollection<string>(channelNames);

        }
        
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            bool contains = CategoryNames.Contains(ctx.Channel.Parent.Name, StringComparer.OrdinalIgnoreCase);

            return CheckMode switch
            {
                ChannelCheckMode.Any => Task.FromResult(contains),
                ChannelCheckMode.None => Task.FromResult(!contains),
                ChannelCheckMode.MineOrParentAny => Task.FromResult(false),
                _ => Task.FromResult(false)
            };


        }
    }
}
