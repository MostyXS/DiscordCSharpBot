using DSharpPlus;
using DSharpPlus.Entities;
using Volodya.Modules.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Volodya.Modules.Dialogue
{
    class DialogueHandler
    {
        private readonly DiscordClient client;
        private readonly DiscordChannel channel;
        private readonly DiscordUser user;
        private  IDialogueStep currentStep;

        public DialogueHandler(DiscordClient client, DiscordChannel channel, DiscordUser user, IDialogueStep startingStep)
        {
            this.client = client;
            this.channel = channel;
            this.user = user;
            currentStep = startingStep;
        }

        private readonly List<DiscordMessage> messages = new List<DiscordMessage>();

        public async Task<bool> ProcessDialogue()
        {
            while(currentStep !=null)
            {
                currentStep.OnMessageAdded += (message) => messages.Add(message);

                bool cancelled = await currentStep.ProcessStep(client, channel, user);
                if(cancelled)
                {
                    await DeleteMessages();

                    var cancelEmbed = new DiscordEmbedBuilder
                    {
                        Title = "The Dialogue Has Successfully being cancelled",
                        Description = user.Mention,
                        Color = DiscordColor.Green
                    };
                    await channel.SendMessageAsync(embed: cancelEmbed);
                }
                currentStep = currentStep.NextStep;
            }

            await DeleteMessages();

            return true;
        }

        private async Task DeleteMessages()
        {
            if (channel.IsPrivate) return;

            foreach(var message in messages)
            {
                await message.DeleteAsync();
            }
        }
    }
}
