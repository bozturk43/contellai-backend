using SocialMediaAssistant.Core.Enums;

namespace SocialMediaAssistant.Core.Entities;

public class ConnectedAccount
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public PlatformType Platform { get; set; }
    public string PlatformUsername { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty; // Bu kesinlikle şifrelenmeli!
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Workspace? Workspace { get; set; }
}