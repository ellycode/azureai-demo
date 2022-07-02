using AzureAIDemoDashboard.Shared;
using Microsoft.JSInterop;

namespace AzureAIDemoDashboard.Client.Shared;

public class SpeechService: IAsyncDisposable
{
    private bool _initialized;
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _speechModule;
    private DotNetObjectReference<SpeechService>? _thisRef;

    public EventHandler<SpeechResult>? SpeechResultCallback { get; set; }

    public SpeechService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitAsync(string locale)
    {
        if (_speechModule is null)
        {
            _speechModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/speech.js");
            _thisRef = DotNetObjectReference.Create(this);
        }

        if (!_initialized)
        {
            _initialized = await _speechModule!.InvokeAsync<bool>("init", _thisRef, locale);
        }
    }

    public ValueTask SynthesizeTextAsync(string text, string locale)
    {
        return !_initialized || string.IsNullOrEmpty(text)
            ? ValueTask.CompletedTask
            : _speechModule!.InvokeVoidAsync("synthesizeText", new SynthesisRequest() { Text = text, Locale = locale });
    }

    public ValueTask StartRecordAsync()
    {
        return !_initialized
            ? ValueTask.CompletedTask
            : _speechModule!.InvokeVoidAsync("startRecord");
    }
    
    public ValueTask StopAsync()
    {
        return _speechModule!.InvokeVoidAsync("stop");
    }

    [JSInvokable]
    public void ShowResult(SpeechResult result)
    {
        SpeechResultCallback?.Invoke(this, result);
    }

    public async ValueTask DisposeAsync()
    {
        _thisRef?.Dispose();
        if (_speechModule != null) await _speechModule.DisposeAsync();
    }
}