using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using NASABot.Accessors.Dialogs;
using NASABot.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NASABot
{
    public class NASABot : IBot
    {
        // The bot state accessor object. Use this to access specific state properties.
        private readonly WelcomeUserStateAccessors _welcomeUserStateAccessors;
        private readonly MultiTurnPromptsAccessor _promptsAccessor;
        private readonly DialogSet _dialogs;
        private const string genericBotMessage = "Welcome to the NASA bot";

        // Initializes a new instance of the <see cref="WelcomeUserBot"/> class.
        public NASABot(WelcomeUserStateAccessors statePropertyAccessor, MultiTurnPromptsAccessor promptsAccessor)
        {
            _welcomeUserStateAccessors = statePropertyAccessor ?? throw new ArgumentNullException("state accessor can't be null");
            _promptsAccessor = promptsAccessor ?? throw new ArgumentNullException("prompts accessor cannot be null");

            _dialogs = new DialogSet(_promptsAccessor.ConversationDialogState);
            _dialogs.SetWaterfallSteps();
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            //set unassigned welcomeUser property to false
            var didBotWelcomeUser = await this._welcomeUserStateAccessors.DidBotWelcomeUser.GetAsync(turnContext, () => false);

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                //Welcome User
                if (didBotWelcomeUser == false)
                {
                    // set bot welcomed user state to true
                    await this._welcomeUserStateAccessors.DidBotWelcomeUser.SetAsync(turnContext, true);
                    //save state
                    await this._welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

                    var userName = turnContext.Activity.From.Name;

                    await turnContext.SendActivityAsync(genericBotMessage);
                }
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("GetData", cancellationToken);
                }

                //save conversation state
                await _promptsAccessor.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                if (results.Status == DialogTurnStatus.Complete)
                {
                    var choiceSelected = results.Result as FoundChoice;
                    await turnContext.SendActivityAsync($"You have chosen {choiceSelected.Value}");
                }
            }
        }
    }
}
