using LuminaryVisuals.Components.ProjectPageDialogue;
using LuminaryVisuals.Components.Shared;
using MudBlazor;

namespace LuminaryVisuals.Services.Shared
{
    public interface IConfirmationService
    {
        Task<bool> Confirm(string message, string title = "Confirm");
    }

    public class ConfirmationService : IConfirmationService
    {
        private readonly IDialogService _dialogService;

        public ConfirmationService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task<bool> Confirm(string message, string title = "Confirm")
        {
            var parameters = new DialogParameters
        {
            { "Message", message }
        };

            var dialog = await _dialogService.ShowAsync<ConfirmationDialog>(title, parameters);
            var result = await dialog.Result;

            return !result.Canceled;
        }
    }
}
