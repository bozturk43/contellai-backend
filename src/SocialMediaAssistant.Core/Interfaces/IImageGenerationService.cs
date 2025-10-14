namespace SocialMediaAssistant.Core.Interfaces;

public interface IImageGenerationService
{
    Task<string> GenerateImageUrlAsync(string prompt);
}