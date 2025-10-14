using System.Net.Http;
using System.Text;
using System.Text.Json;
using SocialMediaAssistant.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SocialMediaAssistant.Infrastructure.Services;

public class HuggingFaceImageGenerationService : IImageGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuggingFaceImageGenerationService> _logger;
    private const string ModelId = "stabilityai/stable-diffusion-xl-base-1.0";

    public HuggingFaceImageGenerationService(HttpClient httpClient, IConfiguration configuration, ILogger<HuggingFaceImageGenerationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var apiKey = configuration["HuggingFace:ApiKey"] ?? throw new InvalidOperationException("Hugging Face API Key not found.");
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.BaseAddress = new Uri($"https://api-inference.huggingface.co/models/");
    }

    public async Task<string> GenerateImageUrlAsync(string prompt)
    {
        try
        {
            _logger.LogInformation("Hugging Face'e '{ModelId}' modeli için istek gönderiliyor...", ModelId);

            var requestBody = new { inputs = $"photorealistic, cinematic, high detail, {prompt}" };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ModelId, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogWarning("Hugging Face modeli şu an yükleniyor, birkaç saniye sonra tekrar deneyin. Hata: {ErrorContent}", errorContent);
                    return "https://via.placeholder.com/1024x1024?text=Model+Loading...+Try+Again";
                }
                _logger.LogError("Hugging Face API Hatası: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var base64String = Convert.ToBase64String(imageBytes);
            var imageUrl = $"data:image/jpeg;base64,{base64String}";

            _logger.LogInformation("Hugging Face'ten görsel başarıyla üretildi ve Base64'e çevrildi.");
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hugging Face'ten görsel üretilirken hata oluştu: {Message}", ex.Message);
            return "https://via.placeholder.com/1024x1024?text=API+Error";
        }
    }
}