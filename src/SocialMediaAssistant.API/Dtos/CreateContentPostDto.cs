using System.ComponentModel.DataAnnotations;
using SocialMediaAssistant.Core.Enums;

public class CreateContentPostDto
{
    [Required]
    public Guid WorkspaceId { get; set; }
    [Required]
    public ContentType ContentType { get; set; }
    [Required]
    [MinLength(10)]
    public string UserPrompt { get; set; } = string.Empty;
}