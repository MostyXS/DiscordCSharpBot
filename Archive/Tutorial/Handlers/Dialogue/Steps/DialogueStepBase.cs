﻿using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Volodya.Modules.Dialogue.Steps
{
    public abstract class DialogueStepBase : IDialogueStep
    {
        protected readonly string content;

        public DialogueStepBase(string content)
        {
            this.content = content;
        }

        public Action<DiscordMessage> OnMessageAdded { get; set; } = delegate { };

        public abstract IDialogueStep NextStep { get; }

        public abstract Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user);

        protected async Task TryAgain (DiscordChannel channel, string problem)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Please Try Again",
                Color = DiscordColor.Red
            };

            embedBuilder.AddField("There was a problem with your previous input", problem);

            var embed = await channel.SendMessageAsync(embed: embedBuilder);

            OnMessageAdded(embed);
        }
        
    }
}
