using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using NASABot.Helpers;
using NASABot.Models;
using NASABot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NASABot.Services
{
    public class DialogConfigurationService : IDialogConfigurationService
    {
        private readonly IDataService dataService;
        private readonly WelcomeUserStateAccessors userStateAccessors;

        public DialogConfigurationService(IDataService service, WelcomeUserStateAccessors userStateAccessors)
        {
            this.dataService = service;
            this.userStateAccessors = userStateAccessors;
        }

        public void SetDialogConfiguration(DialogSet dialogSet)
        {
            AddPictureOfTheDayDialog(dialogSet);
            AddMarsRoverDataDialog(dialogSet);
            AddAsteroidDataDialog(dialogSet);

            //mars rover date prompt
            DateTimePrompt earthDatePrompt = new DateTimePrompt("EarthDatePrompt", DateValidatorAsync);
            dialogSet.Add(earthDatePrompt);

            NumberPrompt<int> numberOfPicturesPrompt = new NumberPrompt<int>("NumberOfPicturesPrompt", NumberValidatorAsync);
            dialogSet.Add(numberOfPicturesPrompt);

            //asteroid start && end date prompts
            DateTimePrompt startDatePrompt = new DateTimePrompt("StartDatePrompt", DateValidatorAsync);
            dialogSet.Add(startDatePrompt);
            DateTimePrompt endDatePrompt = new DateTimePrompt("EndDatePrompt", DateValidatorAsync);
            dialogSet.Add(endDatePrompt);

            //add choice options dialog
            var choicePrompt = new ChoicePrompt("GetChoices");
            dialogSet.Add(choicePrompt);

            //add generic confirmation dialog
            var confirmationPrompt = new ConfirmPrompt("ConfirmationDialog");
            dialogSet.Add(confirmationPrompt);

        }

        private void AddAsteroidDataDialog(DialogSet dialogSet)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ChoiceConfirmationDialogAsync,
                GetStartDateAsync,
                GetEndDateAsync,
                DisplayAsteroidDataAsync
            };

            dialogSet.Add(new WaterfallDialog("DisplayAsteroidData", waterfallSteps));
        }

        #region Asteroids Object Data

        private async Task<DialogTurnResult> GetStartDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Enter a start date"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid date")
                };
                return await stepContext.PromptAsync("StartDatePrompt", options);
            }
            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> GetEndDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dateTimeResolution = (List<DateTimeResolution>)stepContext.Result;
            AsteroidHelper.StartDate = dateTimeResolution.First().Value;

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Enter an end date"),
                RetryPrompt = MessageFactory.Text("Please enter a valid date")
            };
            return await stepContext.PromptAsync("EndDatePrompt", options);
        }

        private async Task<DialogTurnResult> DisplayAsteroidDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string endDate = ((List<DateTimeResolution>)stepContext.Result).First().Value;
            List<Asteroid> asteroids = await this.dataService.GetAsteroids(AsteroidHelper.StartDate, endDate);

            if (asteroids.Count != 0)
            {
                var reply = stepContext.Context.Activity.CreateReply($"Asteroids. Count: {asteroids.Count}");
                reply.Attachments = new List<Attachment>();

                foreach (var asteroid in asteroids)
                {
                    var card = new AdaptiveCard("1.0");
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = asteroid.Name,
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Good
                    });

                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = $"Is hazardous : {asteroid.IsPotentiallyHazardous}",
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Attention
                    });

                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = $"Minimum estimated diameter : {asteroid.MinEstimatedDiameter}",
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Accent
                    });

                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = $"Maximum estimated diameter : {asteroid.MaxEstimatedDiameter}",
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Accent
                    });

                    var attachment = new Attachment
                    {
                        Content = card,
                        ContentType = AdaptiveCard.ContentType
                    };

                    reply.Attachments.Add(attachment);
                }

                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("No asteroids have been found for the dates selected");
            }
            
            return await stepContext.EndDialogAsync();
        }

        #endregion

        #region Mars Rover Dialog

        private void AddMarsRoverDataDialog(DialogSet dialogSet)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ChoiceConfirmationDialogAsync,
                GetEarthDateInputAsync,
                GetNumberOfPicturesToDisplayAsync,
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

        private async Task<DialogTurnResult> GetNumberOfPicturesToDisplayAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            IList<DateTimeResolution> dateTimeResolutions = ((IList<DateTimeResolution>)stepContext.Result);
            string earthDate = dateTimeResolutions.First().Value;
            List<MarsRoverPhoto> marsRoverPictures = await this.dataService.GetMarsRoverPhoto(earthDate);
            if(marsRoverPictures.Count == 0)
            {
                await stepContext.Context.SendActivityAsync("No pictures have been found for the selected date");
                return await stepContext.EndDialogAsync();
            }

            //await this.userStateAccessors.UserData.GetAsync(stepContext.Context, () => new List<MarsRoverPhoto>());
            await this.userStateAccessors.UserData.SetAsync(stepContext.Context, marsRoverPictures);
            await this.userStateAccessors.UserState.SaveChangesAsync(stepContext.Context);

            return await stepContext.PromptAsync("NumberOfPicturesPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text($"Enter a number up to {marsRoverPictures.Count}")                
            });
        }

        private async Task<DialogTurnResult> DisplayDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var marsRoverPhotos = this.userStateAccessors.UserData.GetAsync(stepContext.Context).Result;
            int photoCount = (int)stepContext.Result;
            var reply = stepContext.Context.Activity.CreateReply("Sample pictures");

            var heroCard = new HeroCard
            {
                Title = "Mars Rovers Photos"
            };

            heroCard.Images = new List<CardImage>();

            for (int i = 0; i < photoCount; i++)
            {
                var cardImage = new CardImage
                {
                    Url = marsRoverPhotos[i].ImageSource
                };

                heroCard.Images.Add(cardImage);
            }

            var attachment = new Attachment
            {
                Content = heroCard,
                ContentType = HeroCard.ContentType
            };

            var attachments = new List<Attachment> { attachment };
            reply.Attachments = attachments;
            reply.AttachmentLayout = "list";

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
                Activity reply = stepContext.Context.Activity.CreateReply("Picture of the day");

                var heroCard = new HeroCard
                {
                    Images = new List<CardImage>()
                    {
                        new CardImage() { Url = pictureOfTheDay.Url }
                    },
                    Title = pictureOfTheDay.Title,
                    Text = pictureOfTheDay.Explanation
                };

                var attachments = new List<Attachment>()
                {
                    new Attachment
                    {
                        Content = heroCard,
                        ContentType = HeroCard.ContentType
                    }
                };

                reply.Attachments = attachments;
                await stepContext.Context.SendActivityAsync(reply);
                return await stepContext.EndDialogAsync();
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
        }

        #endregion

        #region Validation Methods

        private async Task<bool> DateValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            //input is not a in date format
            if (promptContext.Recognized.Succeeded == false)
            {
                return false;
            }
            else
            {
                if (promptContext.Recognized.Value.FirstOrDefault().Value != null)
                    return true;
                else
                    return false;
            }
        }


        private async Task<bool> NumberValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
                return true;
            else
                return false;
        }


        #endregion
    }
}
