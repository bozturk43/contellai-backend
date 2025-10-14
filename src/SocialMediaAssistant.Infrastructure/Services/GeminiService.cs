using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using SocialMediaAssistant.Core.Interfaces;
using System.Threading.Tasks;

namespace SocialMediaAssistant.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly GoogleAI _googleAI;

    public GeminiService(IConfiguration configuration)
    {
        var apiKey = configuration["Google:ApiKey"] ?? throw new InvalidOperationException("Google API anahtarı bulunamadı.");
        _googleAI = new GoogleAI(apiKey);
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        var model = _googleAI.GenerativeModel(model:Model.GeminiPro);
        var response = await model.GenerateContent(prompt);
        return response.Text;
    }
}