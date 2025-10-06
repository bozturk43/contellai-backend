namespace SocialMediaAssistant.Application.Interfaces;

public interface IImageGenerationService
{
    Task<string> GenerateImageUrlAsync(string prompt);
}