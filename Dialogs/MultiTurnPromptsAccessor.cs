﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace NASABot.Dialogs
{
    public class MultiTurnPromptsAccessor
    {
        private readonly ConversationState conversationState;

        public MultiTurnPromptsAccessor(ConversationState conversationState)
        {
            this.conversationState = conversationState;
        }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<UserProfile> UserProfile { get; set; }
    }
}
