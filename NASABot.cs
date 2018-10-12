using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
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
        private const string genericBotMessage = "Welcome to the NASA bot \n How should I call you ?";

        // Initializes a new instance of the <see cref="WelcomeUserBot"/> class.
        public NASABot(WelcomeUserStateAccessors statePropertyAccessor, MultiTurnPromptsAccessor promptsAccessor)
        {
            _welcomeUserStateAccessors = statePropertyAccessor ?? throw new System.ArgumentNullException("state accessor can't be null");
            _promptsAccessor = promptsAccessor ?? throw new ArgumentNullException("prompts accessor cannot be null");

            //_dialogs = new DialogSet(_promptsAccessor.)
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            //set unassigned welcomeUser property to false
            var didBotWelcomeUser = await this._welcomeUserStateAccessors.DidBotWelcomeUser.GetAsync(turnContext, () => false);

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (didBotWelcomeUser == false)
                {
                    // set bot welcomed user state to true
                    await this._welcomeUserStateAccessors.DidBotWelcomeUser.SetAsync(turnContext, true);
                    //save state
                    await this._welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

                    var userName = turnContext.Activity.From.Name;

                    await turnContext.SendActivityAsync(genericBotMessage);
                }
                else
                {
                    var messageText = turnContext.Activity.Text;
                    await turnContext.SendActivityAsync($"Hello { messageText}");
                }//if (didBotWelcomeUser == false)
                //{

                //}
            }
            //if (turnContext.Activity.Type is ActivityTypes.Message)
            //{
            //    string userInput = turnContext.Activity.Text;
            //    await turnContext.SendActivityAsync($"You wrote {userInput}");
            //}

            // Generic message to be sent to user

        }
    }
}
