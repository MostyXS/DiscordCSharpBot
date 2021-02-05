using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Threading.Tasks;

namespace LSSKeeper.Handlers.Dialogue.Steps
{
    class IntStep : DialogueStepBase
    {
        private IDialogueStep nextStep;
        private readonly int? minValue;
        private readonly int? maxValue;

        public IntStep(
            string content,
            IDialogueStep nextStep,
            int? minValue = null,
            int? maxValue = null) : base(content)
        {
            this.nextStep = nextStep;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public Action<int> OnValidResult { get; set; } = delegate { };
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

            embedBuilder.AddField("To stop the dialogue", $"Use the cancel");
            if(minValue.HasValue)
            {
                embedBuilder.AddField("Min Value is ", $"{minValue.Value}");
            }
            if (maxValue.HasValue)
            {
                embedBuilder.AddField("Max Value is ", $"{maxValue.Value}");
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
                
                if(!int.TryParse(messageResult.Result.Content, out int inputValue))
                {
                    await TryAgain(channel, $"Your input is not an integer");
                    continue;
                }
                if(minValue.HasValue)
                {
                    if(inputValue < minValue.Value)
                    {
                        await TryAgain(channel, $"Your input value: {inputValue} is smaller than: {minValue}");
                        continue;
                    }
                }
                if (maxValue.HasValue)
                {
                    if (inputValue > maxValue.Value)
                    {
                        await TryAgain(channel, $"Your input value: {inputValue} is larger than: {maxValue}");
                        continue;
                    }
                }
                OnValidResult(inputValue);

                return false;
            }
        }

    }
}
