namespace AzureAIDemoBot.Interfaces;

public interface ISessionsService
{
    List<Session> GetSessionsByTopic(IEnumerable<string> topics);
    List<Session> GetAllSessions();
}