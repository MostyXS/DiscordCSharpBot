using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LSSKeeper.Attributes;
using LSSKeeper.Handlers.Dialogue;
using LSSKeeper.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper.Commands
{
    public class FunCommands : BaseCommandModule
    {
        [Command("guildid")]
        public async Task GetGuildId( CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(ctx.Guild.Id.ToString());
        }

       

        [Command("ping")]
        [Description("Returns pong")]
        public async Task Ping(CommandContext ctx, DiscordRole role, DiscordEmoji emoji  )
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
            
        }

        [Command("add")]
        [Description("Adds two number together")]
        
        public async Task Add(CommandContext ctx, [Description("number one")] int numberOne, int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
            
        }

        /*[Command("respondmessage")]
        public async Task RespondMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            
            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }*/
        [Command("respondreaction")]
        public async Task RespondReaction(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var reaction = await interactivity.WaitForReactionAsync(ctx.Message, ctx.Message.Author, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(reaction.Result.Emoji);
        }
        [Command("poll")]
        [RequireRoles (RoleCheckMode.Any, "Moderator", "Owner")]
        
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());
            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);
            foreach(var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option);
            }
            var result = await interactivity.CollectReactionsAsync(pollMessage, duration);
            var distinctResult = result.Distinct();


            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results));
        }

        [Command("dialogue")]
        public async Task Dialogue(CommandContext ctx)
        {

            var inputStep = new TextStep("Enter something interesting!", null,1 ,150 );
            var funnyStep = new IntStep("Haha, funny", null, maxValue: 100);

            string input = string.Empty;
            int value = 0;

            inputStep.OnValidResult += (result) =>
            {
                input = result;
                if (result == "something interesting")
                {
                    inputStep.SetNextStep(funnyStep);
                }

            };

            funnyStep.OnValidResult += (result) => value = result;


            var userChannel = await ctx.Member.CreateDmChannelAsync();

            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, inputStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue();

            if (!succeeded) return;

            if (succeeded)
                await userChannel.SendMessageAsync(input);


        }
    }
}
//[RequiresRole RoleModeCheck.any/all, "moderator","owner"]
// ctx.Member.GrantRoleAsync(For granting roles by reactions);