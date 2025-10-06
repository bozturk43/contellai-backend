using SocialMediaAssistant.Core.Entities;

namespace SocialMediaAssistant.Application.Interfaces;

public record GeneratedContentResult(string Text, string ImageUrl);

public interface IContentGenerationService
{
    Task<GeneratedContentResult> GenerateContentAsync(string prompt, Workspace context);
}