using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using NASABot.Models;
using NASABot.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
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
            AddPictureOfTheDayDialog(dialogSet);
            AddMarsRoverDataDialog(dialogSet);

            //mars rover date prompt
            DateTimePrompt earthDatePrompt = new DateTimePrompt("EarthDatePrompt", DateValidatorAsync);
            dialogSet.Add(earthDatePrompt);

            //add choice options dialog
            var choicePrompt = new ChoicePrompt("GetChoices");
            dialogSet.Add(choicePrompt);

            //add generic confirmation dialog
            var confirmationPrompt = new ConfirmPrompt("ConfirmationDialog");
            dialogSet.Add(confirmationPrompt);

        }

        #region Mars Rover Dialog

        private async Task<bool> DateValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            //input is not a in date format
            if(promptContext.Recognized.Succeeded == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void AddMarsRoverDataDialog(DialogSet dialogSet)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ChoiceConfirmationDialogAsync,
                GetEarthDateInputAsync,
                DisplayDataAsync
            };

            var dialog = new WaterfallDialog("DisplayMarsRoverData", waterfallSteps);
            dialogSet.Add(dialog);
        }

        private async Task<DialogTurnResult> GetEarthDateInputAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync("EarthDatePrompt", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a date in order to get the appropriate photos"),
                    RetryPrompt = MessageFactory.Text("Please enter the date in the correct format")
                });
            }
            else
            {
                return  await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> DisplayDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            IList<DateTimeResolution> dateTimeResolutions = ((IList<DateTimeResolution>)stepContext.Result);
            string earthDate = dateTimeResolutions.First().Value;
            List<MarsRoverPhoto> marsRoverPictures = await this.dataService.GetMarsRoverPhoto(earthDate);
            var reply = stepContext.Context.Activity.CreateReply("Sample pictures");

            var attachments = new List<Attachment>();
            for (int i = 0; i < 3; i++)
            {
                var attachment = new Attachment
                {
                    ContentType = "image/jpg",
                    ContentUrl = marsRoverPictures[i].ImageSource
                };
                attachments.Add(attachment);
            }

            reply.Attachments = attachments;

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.EndDialogAsync();
        }

        #endregion

        #region Picture Of The Day dialog

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

        #endregion
    }
}
