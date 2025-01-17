using Microsoft.JSInterop;

public class LoadingService
{
    private readonly IJSRuntime _jsRuntime;

    public LoadingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // Existing event system (for server-side components)
    private event Action<bool> OnLoadingChanged;

    public void ShowLoading()
    {
        // Invoke JavaScript to show the loading indicator
        _jsRuntime.InvokeVoidAsync("showLoadingIndicator");
        OnLoadingChanged?.Invoke(true);
    }

    public void HideLoading()
    {
        // Invoke JavaScript to hide the loading indicator
        _jsRuntime.InvokeVoidAsync("hideLoadingIndicator");
        OnLoadingChanged?.Invoke(false);
    }

    public void Subscribe(Action<bool> action)
    {
        OnLoadingChanged += action;
    }
}
