namespace AzureAIDemoBot.Data;

public class WeatherResult
{
    public string Name { get; set; }
    public ulong Dt { get; set; }
    public List<Weather> Weather { get; set; }
    public Main Main { get; set; }
}

public class Weather
{
    public string Main { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class Main
{
    public decimal Temp { get; set; }
    public decimal Temp_Min { get; set; }
    public decimal Temp_Max { get; set; }
}