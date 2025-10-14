using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Application.Models;



namespace SocialMediaAssistant.Application.Interfaces;

public interface IContentGenerationService
{
    Task<GeneratedContentResult> GenerateContentAsync(string prompt, Workspace context);
}