using Microsoft.JSInterop;

namespace LuminaryVisuals.Services.Helpers;

public class BrowserInfoService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserInfoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetUserAgentAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("getUserAgent");
    }
}