using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NASABot.Accessors.Dialogs
{
    public static class DialogConfiguration
    {
        public static void SetWaterfallSteps(this DialogSet dialogs)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                HiStepAsync,
                ChoiceStepAsync
            };

            dialogs.Add(new WaterfallDialog("GetData", waterfallSteps));
            dialogs.Add(new TextPrompt("GetName"));
            dialogs.Add(new ChoicePrompt("choices") { ChoiceOptions = new ChoiceFactoryOptions() { IncludeNumbers = false } });
        }

        private static async Task<DialogTurnResult> ChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("choices", new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select from one of the options"),
                Choices = new List<Choice>() { new Choice("Pluto"), new Choice("Mars"), new Choice("Earth")}
            });
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // a waterfallstep finishes with the end of the waterfall or with another dialog.
            // Running a prompt here means the next WaterfallStep will execute when the user response is received.
            return await stepContext.PromptAsync("GetName", new PromptOptions() { Prompt = MessageFactory.Text("Please enter your name") }, 
                                                        cancellationToken);
        }

        private static async Task<DialogTurnResult> HiStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Hi {stepContext.Result}");

            return await stepContext.ContinueDialogAsync(cancellationToken);
        }
    }
}
