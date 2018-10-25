using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using NASABot.Models;

namespace NASABot.Dialogs
{
    public class MultiTurnPromptsAccessor
    {
        public ConversationState ConversationState{ get; set; }

        public MultiTurnPromptsAccessor(ConversationState conversationState)
        {
            this.ConversationState = conversationState;
        }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<UserProfile> UserProfile { get; set; }
    }
}
