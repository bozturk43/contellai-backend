using System.ComponentModel.DataAnnotations;

namespace SocialMediaAssistant.API.Dtos;

public class UpdateWorkspaceDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string BrandName { get; set; } = string.Empty;

    public string? Industry { get; set; }
    public string? TargetAudience { get; set; }
    public string? BrandTone { get; set; }
    public string? Keywords { get; set; }
}