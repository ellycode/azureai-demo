using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Builder.AI.Luis;

namespace AzureAIDemoBot.Services;

public class IntentRecognizer : IRecognizer
{
    private readonly ILogger<IntentRecognizer> _logger;
    private readonly Dictionary<string, LuisRecognizer> _recognizers;

    public IntentRecognizer(IConfiguration configuration, ILogger<IntentRecognizer> logger)
    {
        _logger = logger;
        var localeConfig = new Dictionary<string, LuisApplication>();
        configuration.GetSection("LUIS").Bind(localeConfig);

        _recognizers = new Dictionary<string, LuisRecognizer>();
        foreach (var (locale, settings) in localeConfig)
        {
            _recognizers.Add(locale, new(new LuisRecognizerOptionsV3(settings)));
        }
    }

    public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var locale = turnContext.Activity.Locale;
        var result = await _recognizers[locale].RecognizeAsync(turnContext, cancellationToken);
        _logger.LogInformation("Intent: {intent}", result.GetTopScoringIntent().intent);
        _logger.LogInformation("Score: {score}", result.GetTopScoringIntent().score);
        foreach (var entity in result.Entities) _logger.LogInformation("Entity: {entity}", entity.Value?.ToString());
        return result;
    }

    public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
        where T : IRecognizerConvert, new()
    {
        var locale = turnContext.Activity.Locale;
        return await _recognizers[locale].RecognizeAsync<T>(turnContext, cancellationToken);
    }
}