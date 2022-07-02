namespace AzureAIDemoBot.Bots;

public class DemoBot<TDialog> : ActivityHandler where TDialog : Dialog
{
    private readonly TDialog _dialog;
    private readonly ConversationState _conversationState;
    private readonly UserState _userState;

    public DemoBot(ConversationState conversationState, UserState userState, TDialog dialog)
    {
        _conversationState = conversationState;
        _userState = userState;
        _dialog = dialog;
    }

    public override async Task OnTurnAsync(ITurnContext turnContext,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes that might have occurred during the turn.
        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        return _dialog.RunAsync(
            turnContext,
            _conversationState.CreateProperty<DialogState>("DialogState"),
            cancellationToken);
    }
}