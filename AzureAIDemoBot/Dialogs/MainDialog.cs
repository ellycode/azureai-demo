namespace AzureAIDemoBot.Dialogs;

public class MainDialog : ComponentDialog
{
    public MainDialog()
    {
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
    }

    private async Task<DialogTurnResult> InitializeStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        await stepContext.LocalizedMessageAsync("None", null, cancellationToken);
        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
    }
}