namespace AzureAIDemoDashboard.Shared;

public class BotRequest
{
    public string Message { get; set; } = "";
    public string UserId { get; set; } = Guid.NewGuid().ToString();
    public string Locale { get; set; } = "it-IT";
}