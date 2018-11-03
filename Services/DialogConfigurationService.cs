using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using NASABot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NASABot.Services
{
    public class DialogConfigurationService : IDialogConfigurationService
    {
        public void SetDialogConfiguration(DialogSet dialogSet)
        {
            SetUserNameInputDialog(dialogSet);
            SetWaterfallSteps(dialogSet);

            //add choice options dialog
            var choicePrompt = new ChoicePrompt("GetChoices");
            dialogSet.Add(choicePrompt);
        }

        private void SetUserNameInputDialog(DialogSet dialogSet)
        {
            var userNameInputWaterfall = new WaterfallStep[]
            {
                GetUserNameAsync,
                EndDialogAsync
            };

            var dialog = new TextPrompt("GetUserName");

            dialogSet.Add(new WaterfallDialog("GetNameOfUser", userNameInputWaterfall));
            dialogSet.Add(dialog);
        }

        private Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(stepContext.Result);
        }

        private async Task<DialogTurnResult> GetUserNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.PromptAsync("GetUserName", new PromptOptions { Prompt = MessageFactory.Text("Please enter you name") });
            return await stepContext.ContinueDialogAsync(cancellationToken);
        }

        private void SetWaterfallSteps(DialogSet dialogSet)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ChoiceStepAsync,
                DisplaySelectedChoiceAsync
            };

            dialogSet.Add(new WaterfallDialog("GetData", waterfallSteps));
            dialogSet.Add(new ChoicePrompt("choices") { ChoiceOptions = new ChoiceFactoryOptions() { IncludeNumbers = false } });
        }

        private async Task<DialogTurnResult> ChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("choices", new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select from one of the options"),
                Choices = new List<Choice>() { new Choice("Pluto"), new Choice("Mars"), new Choice("Earth")}
            });
        }

        private async Task<DialogTurnResult> DisplaySelectedChoiceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // a waterfallstep finishes with the end of the waterfall or with another dialog.
            // Running a prompt here means the next WaterfallStep will execute when the user response is received.
            //return await stepContext.PromptAsync("GetName", new PromptOptions() { Prompt = MessageFactory.Text("Please enter your name") }, 
            //                                            cancellationToken);
            await stepContext.Context.SendActivityAsync($"This is the choice selected {stepContext.Result.ToString()}");
            return await stepContext.EndDialogAsync();
        }
    }
}
