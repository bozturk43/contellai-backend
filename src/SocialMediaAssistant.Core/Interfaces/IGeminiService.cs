namespace SocialMediaAssistant.Core.Interfaces;

public interface IGeminiService
{
    Task<string> GenerateTextAsync(string prompt);
}   