using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AzureAIDemoDashboard.Client;

public class CultureHelper
{
    private readonly IJSRuntime _js;

    public CultureHelper(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<CultureInfo> GetCultureAsync()
    {
        var result = await _js.InvokeAsync<string?>("blazorCulture.get");
        result ??= "en-US";
        Console.Write($@"GetCultureAsync: {result}");
        return new(result);
    }

    public async Task SetCultureAsync(CultureInfo culture)
    {
        Console.Write($@"SetCultureAsync: {culture.Name}");
        await _js.InvokeVoidAsync("blazorCulture.set", culture.Name);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}