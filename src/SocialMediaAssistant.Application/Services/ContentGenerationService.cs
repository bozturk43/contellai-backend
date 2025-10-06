using Mscc.GenerativeAI; // GitHub'daki doğru 'using' ifadesi
using Microsoft.Extensions.Configuration;
using SocialMediaAssistant.Application.Interfaces;
using SocialMediaAssistant.Core.Entities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaAssistant.Application.Services;

public class ContentGenerationService : IContentGenerationService
{
    private readonly IConfiguration _configuration;
    private readonly IImageGenerationService _imageGenerationService;


    public ContentGenerationService(IConfiguration configuration,IImageGenerationService imageGenerationService)
    {
        _configuration = configuration;
        _imageGenerationService = imageGenerationService;

    }

    public async Task<GeneratedContentResult> GenerateContentAsync(string userPrompt, Workspace context)
    {
        var apiKey = _configuration["Google:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Google API anahtarı bulunamadı.");
        }

        // Önce ana 'GoogleAI' nesnesini oluştur
        var ai = new GoogleAI(apiKey);
        
        // Sonra bu nesneden istediğin modeli al
        var model = ai.GenerativeModel(model: Model.GeminiPro);

        // Prompt oluşturma
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Sen, bir sosyal medya içerik üretme asistanısın.");
        promptBuilder.AppendLine($"Aşağıdaki marka kimliğine sahip bir işletme için içerik üreteceksin:");
        promptBuilder.AppendLine($"- Marka Adı: {context.BrandName}");
        promptBuilder.AppendLine($"- Sektör: {context.Industry}");
        promptBuilder.AppendLine($"- Hedef Kitle: {context.TargetAudience}");
        promptBuilder.AppendLine($"- Marka Sesi/Tonu: {context.BrandTone}");
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine("Kullanıcının İsteği:");
        promptBuilder.AppendLine(userPrompt);
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine("Sadece istenen içeriğin metnini üret, ek açıklama veya başlık yapma.");
        var richPrompt = promptBuilder.ToString();
        
        var response = await model.GenerateContent(richPrompt);

        var generatedText = response.Text;

        var imageSearchPrompt = userPrompt.Length > 50 ? userPrompt.Substring(0, 50) : userPrompt;
        var imageUrl = await _imageGenerationService.GenerateImageUrlAsync(imageSearchPrompt);

        return new GeneratedContentResult(generatedText ?? "AI bir metin üretemedi.", imageUrl);
    }
}