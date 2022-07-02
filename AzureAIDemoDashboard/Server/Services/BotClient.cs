namespace AzureAIDemoDashboard.Server.Services;

public class BotClient
{
    private readonly HttpClient _http;

    public BotClient()
    {
        _http = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:3978/api/messages"),  
        };
    }

    public async Task<List<BotResponse>> SendMessage(BotRequest request, CancellationToken cancellationToken=default)
    {
        var activity = new Activity()
        {
            Type = ActivityTypes.Message,
            Text = request.Message,
            DeliveryMode = DeliveryModes.ExpectReplies,
            From = new ChannelAccount() { Id = request.UserId, Name = "user", Role = "user" },
            Recipient = new ChannelAccount() { Id = "bot", Name = "bot", Role = "bot" },
            ChannelId = "elly-dashboard",
            Conversation = new ConversationAccount() { Id = $"conv-{request.UserId}" },
            ServiceUrl = "http://localhost",
            Locale = request.Locale,
            LocalTimestamp = DateTime.UtcNow,
        };

        var response = await _http.PostAsJsonAsync("", activity, cancellationToken);
        var jsonBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonDoc = JsonDocument.Parse(jsonBody);
        var activities = jsonDoc.RootElement.GetProperty("activities")
            .EnumerateArray()
            .Select(x => JsonConvert.DeserializeObject<Activity>(x.GetRawText()))
            .Cast<Activity>()
            .Where(x => x.Type == ActivityTypes.Message)
            .Select(ToBotResponse)
            .ToList();

        return activities;
    }

    private BotResponse ToBotResponse(Activity activity)
    {
        string? tableJson = null;
        string? cardJson = null;
        
        if (activity.Attachments is not null)
        {
            var tableAttachment = activity.Attachments.FirstOrDefault(x => x.ContentType == "application/my-data-content");
            tableJson = tableAttachment?.Content?.ToString();
            
            var cardAttachment = activity.Attachments.FirstOrDefault(
                x => x.ContentType == "application/vnd.microsoft.card.adaptive");
            cardJson = cardAttachment?.Content?.ToString();
        }

        return new BotResponse
        {
            ResponseText = activity.Text,
            ResponseExpected = activity.InputHint == InputHints.ExpectingInput,
            ResponseTable = tableJson,
            ResponseCard = cardJson,
        };
    }
}