using Newtonsoft.Json.Linq;

namespace AzureAIDemoBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly GreetingsDialog _greetingsDialog;
    private readonly LookupSessionDialog _lookupSessionDialog;
    private readonly WeatherForecastDialog _weatherForecastDialog;
    private readonly IRecognizer _recognizer;

    public MainDialog(
        GreetingsDialog greetingsDialog,
        LookupSessionDialog lookupSessionDialog,
        WeatherForecastDialog weatherForecastDialog,
        IRecognizer recognizer)
    {
        _greetingsDialog = greetingsDialog;
        _lookupSessionDialog = lookupSessionDialog;
        _weatherForecastDialog = weatherForecastDialog;
        _recognizer = recognizer;
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
        var luisResult = await _recognizer.RecognizeAsync(stepContext.Context, cancellationToken);
        var intent = luisResult.GetTopScoringIntent();

        switch (intent.intent)
        {
            // Greetings dialog 
            case "Greetings":
            {
                return await stepContext.BeginDialogAsync(nameof(GreetingsDialog), null, cancellationToken);
            }

            // LookupSession dialog
            case "LookupSession":
            {
                var topics = new List<string>();
                luisResult.Entities.TryGetValue("keyPhrase", out var keyPhrases);
                if(keyPhrases is JArray)
                    topics.AddRange(keyPhrases.Values<string>());

                return await stepContext.BeginDialogAsync(nameof(LookupSessionDialog), topics, cancellationToken);
            }

            // Weather dialog
            case "Weather_QueryWeather":
            {
                var parameters = new Dictionary<string, string>
                {
                    ["cityName"] = luisResult.Entities["Places_AbsoluteLocation"]?.ToString(),
                };
                return await stepContext.BeginDialogAsync(nameof(WeatherForecastDialog), parameters, cancellationToken);
            }

            // Fallback message
            default:
                await stepContext.LocalizedMessageAsync("None", null, cancellationToken);
                break;
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