using AdaptiveCards.Templating;

namespace AzureAIDemoBot.Dialogs;

public class WeatherForecastDialog : ComponentDialog
{
    private readonly IWeatherService _weatherService;

    public WeatherForecastDialog(IWeatherService weatherService) : base(nameof(WeatherForecastDialog))
    {
        _weatherService = weatherService;
        InitializeWaterfallDialog();
    }

    private void InitializeWaterfallDialog()
    {
        var waterfallSteps = new WaterfallStep[]
        {
            AskCityNameStepAsync,
            FinalStepAsync,
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new WaterfallDialog($"{nameof(WeatherForecastDialog)}.main", waterfallSteps));
        InitialDialogId = $"{nameof(WeatherForecastDialog)}.main";
    }

    private async Task<DialogTurnResult> AskCityNameStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var parameters = stepContext.Options as Dictionary<string, string> ?? new();
        parameters.TryGetValue("cityName", out var cityName);

        if (string.IsNullOrEmpty(cityName))
        {
            return await stepContext.LocalizedPromptAsync("WeatherForecast_AskCity", null, cancellationToken);
        }

        return await stepContext.NextAsync(cityName, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var cityName = (string) stepContext.Result;
        cityName = new(cityName.Where(char.IsLetter).ToArray());

        // The current user's locale (ex. it-IT)
        var locale = stepContext.GetLocaleOrDefault();
        var weather = await _weatherService.GetCurrentWeatherAsync(
            cityName, locale[..2], cancellationToken);

        var template = GetAdaptiveCardTemplate("WeatherCard", locale);
        var cardAttachment = CreateAdaptiveCard(template, weather);
        await stepContext.Context.SendActivityAsync(
            MessageFactory.Attachment(cardAttachment),
            cancellationToken);

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private static AdaptiveCardTemplate GetAdaptiveCardTemplate(string name, string locale)
    {
        // Create a Template instance from the template payload
        var filePath = Path.Combine(".", "Resources", $"{name}.{locale}.json");
        var adaptiveCardJson = File.ReadAllText(filePath);
        var template = new AdaptiveCardTemplate(adaptiveCardJson);
        return template;
    }

    private static Attachment CreateAdaptiveCard(AdaptiveCardTemplate template, object cardData)
    {
        // Render the Template with the data
        var cardJson = template.Expand(cardData);

        // Return it to the client as an Attachment
        var adaptiveCardAttachment = new Attachment()
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(cardJson),
        };
        return adaptiveCardAttachment;
    }
}