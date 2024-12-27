// LoadingService.cs
using LuminaryVisuals.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class LoadingService
{
    private readonly IJSRuntime _jsRuntime;
    private event Action<bool> OnLoadingChanged;

    public LoadingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public void ShowLoading() => OnLoadingChanged?.Invoke(true);
    public void HideLoading() => OnLoadingChanged?.Invoke(false);
    public void Subscribe(Action<bool> action) => OnLoadingChanged += action;
}
