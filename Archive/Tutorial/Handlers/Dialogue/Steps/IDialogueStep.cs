﻿using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper.Handlers.Dialogue.Steps
{
    public interface IDialogueStep
    {
        Action<DiscordMessage> OnMessageAdded { get; set; }
        IDialogueStep NextStep { get; }
        Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user);
        
        
    }
}
