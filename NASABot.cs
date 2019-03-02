using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using NASABot.Dialogs;
using NASABot.Models;
using NASABot.Services.Interfaces;
using System;
using System.Collections.Generic;
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

        public NASABot(WelcomeUserStateAccessors statePropertyAccessor, MultiTurnPromptsAccessor promptsAccessor, 
            IDialogConfigurationService dialogConfigurationService)
        {
            _welcomeUserStateAccessors = statePropertyAccessor ?? throw new ArgumentNullException("state accessor can't be null");
            _promptsAccessor = promptsAccessor ?? throw new ArgumentNullException("prompts accessor cannot be null");

            _dialogs = new DialogSet(_promptsAccessor.ConversationDialogState);        
            dialogConfigurationService.SetDialogConfiguration(_dialogs);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // set unassigned welcomeUser and user Profile name to default values
            await this._welcomeUserStateAccessors.UserData.GetAsync(turnContext, () => new List<MarsRoverPhoto>());
            var didBotWelcomeUser = await this._welcomeUserStateAccessors.DidBotWelcomeUser.GetAsync(turnContext, () => false);
            await this._welcomeUserStateAccessors.UserProfile.GetAsync(turnContext, () => null);
   
            string userName = this._welcomeUserStateAccessors.UserProfile.GetAsync(turnContext).Result;

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // get dialog context
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                //welcome User
                if (didBotWelcomeUser == false)
                {
                    await WelcomeUser(turnContext);
                }
                // get userName
                else if (string.IsNullOrEmpty(userName) && didBotWelcomeUser)
                {
                    await SetUserProfileName(turnContext);

                    ////present dialog options for user choose from
                    await dialogContext.PromptAsync("GetChoices", new PromptOptions()
                    {
                        Choices = new List<Choice>() { new Choice(nameof(PictureOfTheDay)), new Choice(nameof(MarsRoverPhoto)), new Choice(nameof(Asteroid)) },
                        Prompt = MessageFactory.Text("Select from one of the options")
                    });
                    
                }
                // display information based on user selection
                else
                {
                    //var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                    if (dialogResult != null && dialogResult.Status == DialogTurnStatus.Empty || 
                        dialogResult.Status == DialogTurnStatus.Complete)
                    {
                        if (turnContext.Activity.Text == nameof(PictureOfTheDay))
                        {
                            await dialogContext.BeginDialogAsync("ShowPictureOfTheDay");

                            //await this._promptsAccessor.ConversationState.SaveChangesAsync(turnContext, true);
                        }
                        else if (turnContext.Activity.Text == nameof(MarsRoverPhoto))
                        {
                            await dialogContext.BeginDialogAsync("DisplayMarsRoverData");

                            //await this._promptsAccessor.ConversationState.SaveChangesAsync(turnContext, true);
                        }
                        else if(turnContext.Activity.Text == nameof(Asteroid))
                        {
                            await dialogContext.BeginDialogAsync("DisplayAsteroidData");
                        }
                    }
                }
                // Save the dialog state into the conversation state.
                await _promptsAccessor.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
        }

        private async Task SetUserProfileName(ITurnContext turnContext)
        {
            string userNameInput = turnContext.Activity.Text;
            await this._welcomeUserStateAccessors.UserProfile.SetAsync(turnContext, userNameInput);

            await this._welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

            await turnContext.SendActivityAsync($"Hello { userNameInput }");
        }

        private async Task WelcomeUser(ITurnContext turnContext)
        {
            // set bot welcomed user state to true
            await this._welcomeUserStateAccessors.DidBotWelcomeUser.SetAsync(turnContext, true);

            // save state
            await this._welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

            await turnContext.SendActivityAsync("Hello");
            await turnContext.SendActivityAsync("How shall I call you ?");
        }
    }
}
