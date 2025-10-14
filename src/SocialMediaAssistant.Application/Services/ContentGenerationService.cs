using Microsoft.Extensions.Configuration;
using SocialMediaAssistant.Application.Interfaces;
using SocialMediaAssistant.Application.Models;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaAssistant.Application.Services;

public class ContentGenerationService : IContentGenerationService
{
    private readonly IGeminiService _geminiService;
    private readonly IImageGenerationService _imageGenerationService;

    public ContentGenerationService(IImageGenerationService imageGenerationService,IGeminiService geminiService)
    {
        _geminiService = geminiService;
        _imageGenerationService = imageGenerationService;
    }

    public async Task<GeneratedContentResult> GenerateContentAsync(string userPrompt, Workspace context)
    {
        var imagePromptTask = GenerateImagePromptAsync(userPrompt, context);

        var socialTextTask = GenerateSocialTextAsync(userPrompt, context);

        await Task.WhenAll(imagePromptTask, socialTextTask);

        var imagePrompt = await imagePromptTask;
        var socialText = await socialTextTask;

        var imageUrl = await _imageGenerationService.GenerateImageUrlAsync(imagePrompt);

        return new GeneratedContentResult(socialText, imageUrl);
    }

    private async Task<string> GenerateSocialTextAsync(string userPrompt, Workspace context)
    {
        
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
        
        return await _geminiService.GenerateTextAsync(promptBuilder.ToString());

    }

    private async Task<string> GenerateImagePromptAsync(string userPrompt, Workspace context)
    {        
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Bir yapay zeka görsel üretme servisi için İngilizce bir prompt oluştur.");
        promptBuilder.AppendLine("Prompt, aşağıdaki Türkçe isteği ve marka kimliğini yansıtmalı.");
        promptBuilder.AppendLine("Prompt detaylı, sinematik ve fotogerçekçi olmalı.");
        promptBuilder.AppendLine("Sadece İngilizce prompt'u yaz, başka hiçbir açıklama yapma.");
        promptBuilder.AppendLine($"Marka Sektörü: {context.Industry}");
        promptBuilder.AppendLine($"Kullanıcı İsteği: {userPrompt}");

        var result = await _geminiService.GenerateTextAsync(promptBuilder.ToString());
        return string.IsNullOrEmpty(result) ? userPrompt : result;
    }
}