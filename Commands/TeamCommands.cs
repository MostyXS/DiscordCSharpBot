using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;

namespace LSSKeeper.Commands
{
    class TeamCommands : BaseCommandModule
    {
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = "Would you like to join",
                ImageUrl = ctx.Client.CurrentUser.AvatarUrl,
                Color = DiscordColor.Green
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);
            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var reaction = await interactivity.WaitForReactionAsync(x => x.Message == joinMessage && x.User == ctx.User && (x.Emoji == thumbsUpEmoji || x.Emoji == thumbsDownEmoji));
            if (reaction.Result.Emoji == thumbsUpEmoji)
            {

                var role = ctx.Guild.GetRole(780866654791532634);
                await ctx.Member.GrantRoleAsync(role);

            }
            await joinMessage.DeleteAsync();
        }


    }
}
