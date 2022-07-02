namespace AzureAIDemoBot.Interfaces;

public interface IWeatherService
{
    Task<WeatherResult> GetCurrentWeatherAsync(string cityName, string language, CancellationToken cancellationToken);
}