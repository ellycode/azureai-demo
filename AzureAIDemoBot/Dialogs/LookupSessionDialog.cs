namespace AzureAIDemoBot.Dialogs;

public class LookupSessionDialog : ComponentDialog
{
    private readonly ISessionsService _sessionsService;

    public LookupSessionDialog(ISessionsService sessionsService) : base(nameof(LookupSessionDialog))
    {
        _sessionsService = sessionsService;
        InitializeWaterfallDialog();
    }

    private void InitializeWaterfallDialog()
    {
        var waterfallSteps = new WaterfallStep[]
        {
            AskSpeakerNameStepAsync,
            FinalStepAsync
        };

        AddDialog(new WaterfallDialog($"{nameof(LookupSessionDialog)}.main", waterfallSteps));
        InitialDialogId = $"{nameof(LookupSessionDialog)}.main";
    }

    private async Task<DialogTurnResult> AskSpeakerNameStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var topics = stepContext.Options as List<string> ?? new();

        var sessions = topics.Any()
            ? _sessionsService.GetSessionsByTopic(topics)
            : _sessionsService.GetAllSessions();

        if (sessions.Any())
        {
            await stepContext.LocalizedMessageAsync("LookupSession_FoundTheseSessions", null, cancellationToken);
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Attachment(new Attachment(
                    contentType: "application/my-data-content",
                    content: JsonConvert.SerializeObject(sessions))),
                cancellationToken);
        }
        else
        {
            await stepContext.LocalizedMessageAsync("LookupSession_FoundNothing", null, cancellationToken);
        }

        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}