namespace AzureAIDemoDashboard.Shared;

public class BotResponse
{
    public string ResponseText { get; set; } = "";
    public string? ResponseCard { get; set; }
    public string? ResponseTable { get; set; }
    public bool ResponseExpected { get; set; }
}