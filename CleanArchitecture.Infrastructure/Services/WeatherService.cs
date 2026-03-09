using System.Net.Http.Json;
using CleanArchitecture.Application.Abstractions.Services;

namespace CleanArchitecture.Infrastructure.Services;

internal sealed class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherData> GetCurrentWeatherAsync(string location, CancellationToken cancellationToken = default)
    {
        // 实际场景下这会访问一个真实第三方的 API 如 https://api.weatherapi.com
        // var response = await _httpClient.GetFromJsonAsync<WeatherData>($"weather?q={location}", cancellationToken);
        // return response!;
        
        // 此处为示范：
        await Task.Delay(100, cancellationToken);
        return new WeatherData(location, 25.5);
    }
}
