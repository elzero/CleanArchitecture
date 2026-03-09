namespace CleanArchitecture.Application.Abstractions.Services;

public record WeatherData(string Location, double TemperatureCelsius);

public interface IWeatherService
{
    Task<WeatherData> GetCurrentWeatherAsync(string location, CancellationToken cancellationToken = default);
}
