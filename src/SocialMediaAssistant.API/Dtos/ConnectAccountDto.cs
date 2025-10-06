using SocialMediaAssistant.Core.Enums;
using System.ComponentModel.DataAnnotations;

public class ConnectAccountDto
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    public PlatformType Platform { get; set; }

    [Required]
    public string PlatformUsername { get; set; } = string.Empty;
}