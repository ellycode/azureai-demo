namespace AzureAIDemoBot.Dialogs;

public class GreetingsDialog : ComponentDialog
{
    private readonly UserState _userState;

    public GreetingsDialog(UserState userState) : base(nameof(GreetingsDialog))
    {
        _userState = userState;
        InitializeWaterfallDialog();
    }

    private void InitializeWaterfallDialog()
    {
        var waterfallSteps = new WaterfallStep[]
        {
            AskUserNameStepAsync,
            FinalStepAsync,
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new WaterfallDialog($"{nameof(GreetingsDialog)}.main", waterfallSteps));
        InitialDialogId = $"{nameof(GreetingsDialog)}.main";
    }

    private async Task<DialogTurnResult> AskUserNameStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
        var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new(), cancellationToken);

        if (string.IsNullOrEmpty(userProfile.Name))
        {
            return await stepContext.LocalizedPromptAsync("Greetings_AskName", null, cancellationToken);
        }

        await stepContext.LocalizedMessageAsync(
            "Greetings_WelcomeBack",
            new {name = userProfile.Name},
            cancellationToken);
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var userName = stepContext.Result as string;
        if (string.IsNullOrEmpty(userName))
            return await stepContext.EndDialogAsync(null, cancellationToken);

        var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
        var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new(), cancellationToken);
        userProfile.Name = userName;

        await userStateAccessors.SetAsync(stepContext.Context, userProfile, cancellationToken);

        await stepContext.LocalizedPromptAsync(
            "Greetings_NiceToMeetYou",
            new {name = userProfile.Name},
            cancellationToken);
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}