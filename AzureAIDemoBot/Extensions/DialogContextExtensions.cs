using Microsoft.Bot.Builder.LanguageGeneration;

namespace AzureAIDemoBot.Extensions;

public static class DialogContextExtensions
{
    private static readonly MultiLanguageLG MultiLanguageGen;

    static DialogContextExtensions()
    {
        var supportedLocales = new[] {"it-IT", "en-US"};
        var templates = new Dictionary<string, Templates>();
        foreach (var locale in supportedLocales)
        {
            templates.Add(locale, GetLanguageResource(locale));
        }

        MultiLanguageGen = new(templates);
    }

    public static string GetLocaleOrDefault(this DialogContext stepContext) =>
        string.IsNullOrEmpty(stepContext.GetLocale())
            ? "it-IT"
            : stepContext.GetLocale();

    private static Templates GetLanguageResource(string locale)
    {
        var filePath = Path.Combine(".", "Resources", $"Locale.{locale}.lg");
        var languageResourceContent = File.ReadAllText(filePath);
        var lgResource = new LGResource(locale, locale, languageResourceContent);
        return Templates.ParseResource(lgResource);
    }

    public static async Task LocalizedMessageAsync(this DialogContext context,
        string templateName,
        object data,
        CancellationToken cancellationToken = default)
    {
        var message = MultiLanguageGen.Generate(templateName, data, context.GetLocaleOrDefault());
        var activity = ActivityFactory.FromObject(message);
        activity.InputHint = InputHints.IgnoringInput;
        await context.Context.SendActivityAsync(activity, cancellationToken);
    }

    public static async Task<DialogTurnResult> LocalizedPromptAsync(
        this DialogContext context,
        string templateName,
        object data,
        CancellationToken cancellationToken = default)
    {
        var message = MultiLanguageGen.Generate(templateName, data, context.GetLocaleOrDefault());
        return await context.PromptAsync(
            nameof(TextPrompt),
            new() {Prompt = ActivityFactory.FromObject(message)},
            cancellationToken);
    }
}