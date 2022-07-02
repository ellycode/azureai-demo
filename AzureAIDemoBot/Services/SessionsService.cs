namespace AzureAIDemoBot.Services;

public class SessionsService : ISessionsService
{
    private static readonly List<Session> SessionsData;

    static SessionsService()
    {
        var filePath = Path.Combine(".", "Resources", "SessionsData.json");
        if (File.Exists(filePath))
        {
            using var fs = File.OpenRead(filePath);
            SessionsData = System.Text.Json.JsonSerializer.Deserialize<List<Session>>(fs);
        }
        else
        {
            SessionsData = new();
        }
    }

    public List<Session> GetSessionsByTopic(IEnumerable<string> topics)
    {
        var topicsLower = topics
            .Select(t => t.ToLower().Trim())
            .Distinct()
            .ToArray();

        var sessions = new List<Session>();
        foreach (var topic in topicsLower)
        {
            sessions.AddRange(SessionsData.Where(s => 
                s.Title.ToLower().Contains(topic) || 
                s.Speakers.ToLower().Contains(topic)));
        }

        return sessions;
    }

    public List<Session> GetAllSessions() => new (SessionsData);
}