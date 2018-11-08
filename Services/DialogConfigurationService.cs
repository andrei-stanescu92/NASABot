using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using NASABot.Services.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NASABot.Services
{
    public class DialogConfigurationService : IDialogConfigurationService
    {
        private readonly IDataService dataService;

        public DialogConfigurationService(IDataService service)
        {
            this.dataService = service;
        }

        public void SetDialogConfiguration(DialogSet dialogSet)
        {
            SetUserNameInputDialog(dialogSet);
            SetWaterfallSteps(dialogSet);
            AddPictureOfTheDayDialog(dialogSet);

            //add choice options dialog
            var choicePrompt = new ChoicePrompt("GetChoices");
            dialogSet.Add(choicePrompt);

            //add generic confirmation dialog
            var confirmationPrompt = new ConfirmPrompt("ConfirmationDialog");
            dialogSet.Add(confirmationPrompt);
        }

        private void AddPictureOfTheDayDialog(DialogSet dialogSet)
        {
            var dialogSteps = new WaterfallStep[]
            {
                ChoiceConfirmationDialogAsync,
                DisplayPictureAsync
            };

            dialogSet.Add(new WaterfallDialog("ShowPictureOfTheDay", dialogSteps));
        }

        private async Task<DialogTurnResult> ChoiceConfirmationDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            PromptOptions options = new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Your choice is {stepContext.Context.Activity.Text}. Is that correct ?")
            };
            return await stepContext.PromptAsync("ConfirmationDialog", options);
        }

        private async Task<DialogTurnResult> DisplayPictureAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // return picture object if user confirms choice selected. Otherwise end dialog
            if ((bool)stepContext.Result == true)
            {
                var pictureOfTheDay = await this.dataService.GetCurrentPictureOfTheDay();

                // create and send reply to user containing the returned API data
                Activity reply = stepContext.Context.Activity.CreateReply(pictureOfTheDay.Title);
                reply.Summary = pictureOfTheDay.Explanation;
                var attachments = new List<Attachment>()
                    {
                        new Attachment
                        {
                            ContentUrl = pictureOfTheDay.Url,
                            ContentType = "image/jpg"
                        }
                    };

                reply.Attachments = attachments;
                await stepContext.Context.SendActivityAsync(reply);
                await stepContext.Context.SendActivityAsync(reply.Summary);
                return await stepContext.EndDialogAsync();
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
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
