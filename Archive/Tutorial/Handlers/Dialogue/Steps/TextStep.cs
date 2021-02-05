using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Threading.Tasks;

namespace LSSKeeper.Handlers.Dialogue.Steps
{
    class TextStep : DialogueStepBase
    {
        private IDialogueStep nextStep;
        private readonly int? minLength;
        private readonly int? maxLength;

        public TextStep(
            string content,
            IDialogueStep nextStep,
            int? minLength = null,
            int? maxLength = null) : base(content)
        {
            this.nextStep = nextStep;
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        public Action<string> OnValidResult { get; set; } = delegate { };
        public override IDialogueStep NextStep => nextStep;

        public void SetNextStep(IDialogueStep nextStep)
        {
            this.nextStep = nextStep;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Please Respond Below",
                Description = $"{user.Mention}, {content}"

            };

            embedBuilder.AddField("To stop the dialogue", $"Use the cancel command");
            if(minLength.HasValue)
            {
                embedBuilder.AddField("Min Length is ", $"{minLength.Value} characters");
            }
            if (maxLength.HasValue)
            {
                embedBuilder.AddField("Max Length is ", $"{maxLength.Value} characters");
            }

            var interactivity = client.GetInteractivity();

            while(true)
            {
                var embed = await channel.SendMessageAsync(embed: embedBuilder);
                OnMessageAdded(embed);

                var messageResult = await interactivity.WaitForMessageAsync(x => x.Channel.Id == channel.Id && x.Author.Id == user.Id);

                OnMessageAdded(messageResult.Result);
                if(messageResult.Result.Content.Equals("!cancel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if(minLength.HasValue)
                {
                    if(messageResult.Result.Content.Length < minLength.Value)
                    {
                        var difference = minLength.Value - messageResult.Result.Content.Length;
                        await TryAgain(channel, $"Your input is {difference} characters too short");
                        continue;
                    }
                }
                if (maxLength.HasValue)
                {
                    if (messageResult.Result.Content.Length > maxLength.Value)
                    {
                        var difference = messageResult.Result.Content.Length - maxLength.Value;
                        await TryAgain(channel, $"Your input is {difference} characters too long");
                        continue;
                    }
                }
                OnValidResult(messageResult.Result.Content);

                return false;
            }
        }

    }
}
