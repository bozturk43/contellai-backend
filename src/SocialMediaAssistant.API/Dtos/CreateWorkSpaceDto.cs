using System.ComponentModel.DataAnnotations;

namespace SocialMediaAssistant.API.Dtos;

public class CreateWorkspaceDto
{
    [Required]
    public string BrandName { get; set; } = string.Empty;

    public string? Industry { get; set; }
    public string? TargetAudience { get; set; }
    public string? BrandTone { get; set; }
    public string? Keywords { get; set; }
}