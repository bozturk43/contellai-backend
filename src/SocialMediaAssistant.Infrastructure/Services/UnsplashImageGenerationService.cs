using SocialMediaAssistant.Core.Interfaces;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging;

namespace SocialMediaAssistant.Core.Services;

public class UnsplashImageGenerationService : IImageGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessKey;
    private readonly ILogger<UnsplashImageGenerationService> _logger;

    public UnsplashImageGenerationService(HttpClient httpClient, IConfiguration configuration, ILogger<UnsplashImageGenerationService> logger)
    {
        _httpClient = httpClient;
        _accessKey = configuration["Unsplash:AccessKey"] ?? 
                     throw new InvalidOperationException("Unsplash Access Key not found in configuration.");
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_accessKey}");
        _httpClient.BaseAddress = new Uri("https://api.unsplash.com/");
    }

    public async Task<string> GenerateImageUrlAsync(string prompt)
    {
        try
        {
            // Unsplash API'sinde görselleri aramak için "search/photos" endpoint'ini kullanıyoruz
            // prompt'taki kelimeleri anahtar kelime olarak kullanırız
            var response = await _httpClient.GetAsync($"search/photos?query={Uri.EscapeDataString(prompt)}&per_page=1");
            response.EnsureSuccessStatusCode(); // HTTP 200-299 aralığında değilse hata fırlat

            var jsonString = await response.Content.ReadAsStringAsync();

            // Gelen JSON'ı ayrıştırmak için basit bir anonymous type kullanabiliriz.
            // Sadece ihtiyacımız olan "urls" içindeki "regular" alanını alacağız.
            var data = JsonSerializer.Deserialize<JsonDocument>(jsonString);
            var imageUrl = data?.RootElement
                               .GetProperty("results")
                               .EnumerateArray()
                               .FirstOrDefault()
                               .GetProperty("urls")
                               .GetProperty("regular")
                               .GetString();

            if (!string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogInformation($"Unsplash'ten görsel bulundu: {imageUrl}");
                return imageUrl;
            }

            _logger.LogWarning($"Unsplash'ten '{prompt}' için görsel bulunamadı. Varsayılan görsel kullanılıyor.");
            return "https://via.placeholder.com/600x400?text=No+Image+Found"; // Varsayılan görsel
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unsplash'ten görsel alınırken hata oluştu: {ex.Message}");
            return "https://via.placeholder.com/600x400?text=Image+Error"; // Hata durumunda varsayılan görsel
        }
    }
}