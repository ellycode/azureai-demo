namespace AzureAIDemoBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly GreetingsDialog _greetingsDialog;
    private readonly LookupSessionDialog _lookupSessionDialog;
    private readonly WeatherForecastDialog _weatherForecastDialog;

    public MainDialog(
        GreetingsDialog greetingsDialog,
        LookupSessionDialog lookupSessionDialog,
        WeatherForecastDialog weatherForecastDialog)
    {
        _greetingsDialog = greetingsDialog;
        _lookupSessionDialog = lookupSessionDialog;
        _weatherForecastDialog = weatherForecastDialog;
        InitializeWaterfallDialog();
    }

    private void InitializeWaterfallDialog()
    {
        var waterfallSteps = new WaterfallStep[]
        {
            InitializeStepAsync,
            FinalStepAsync,
        };

        InitialDialogId = $"{nameof(MainDialog)}.main";
        AddDialog(new WaterfallDialog(InitialDialogId, waterfallSteps));
        AddDialog(_greetingsDialog);
        AddDialog(_lookupSessionDialog);
        AddDialog(_weatherForecastDialog);
    }

    private async Task<DialogTurnResult> InitializeStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var utterance = stepContext.Context.Activity.Text?.ToLower() ?? "";
        
        // Greetings dialog 
        if (utterance.Contains("ciao") || utterance.Contains("hello"))
        {
            return await stepContext.BeginDialogAsync(nameof(GreetingsDialog), null, cancellationToken);
        }
        
        // LookupSession dialog 
        else if (utterance.Contains("session"))
        {
            return await stepContext.BeginDialogAsync(nameof(LookupSessionDialog), null, cancellationToken);
        }
        
        // Weather dialog 
        if (utterance.Contains("meteo") || utterance.Contains("weather"))
        {
            return await stepContext.BeginDialogAsync(nameof(WeatherForecastDialog), null, cancellationToken);
        }

        // Fallback message
        else
        {
            await stepContext.LocalizedMessageAsync("None", null, cancellationToken);
        }

        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
    }
}