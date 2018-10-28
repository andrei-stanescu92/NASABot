using Microsoft.Bot.Builder.Dialogs;

namespace NASABot.Services.Interfaces
{
    public interface IDialogConfigurationService
    {
        void SetDialogConfiguration(DialogSet dialogSet);
    }
}