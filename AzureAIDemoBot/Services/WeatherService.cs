namespace AzureAIDemoBot.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly string _openWeatherMapApiKey;

    public WeatherService(IConfiguration configuration, HttpClient http)
    {
        _http = http;
        _openWeatherMapApiKey = configuration["OpenWeatherMapAPIKey"];
    }

    public Task<WeatherResult> GetCurrentWeatherAsync(
        string cityName, 
        string language, 
        CancellationToken cancellationToken)
    {
        var uri = @"https://api.openweathermap.org/data/2.5/weather" +
                  $"?q={cityName}" +
                  $"&appid={_openWeatherMapApiKey}" +
                  @"&units=metric" +
                  $"&lang={language}";
        
        return _http.GetFromJsonAsync<WeatherResult>(uri, cancellationToken);
    }
}
