using System.ComponentModel.DataAnnotations;
using SocialMediaAssistant.Core.Enums;


public class SaveContentPostDto
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    public string UserPrompt { get; set; } = string.Empty;
    
    public ContentType ContentType { get; set; }

    [Required]
    public string GeneratedText { get; set; } = string.Empty;

    [Required]
    public string GeneratedAssetUrl { get; set; } = string.Empty;
}