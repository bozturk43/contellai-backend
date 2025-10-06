using SocialMediaAssistant.Core.Enums;

namespace SocialMediaAssistant.Core.Entities;

public class ContentPost
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public ContentType ContentType { get; set; }
    public string UserPrompt { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public string GeneratedAssetUrl { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public PostStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Workspace? Workspace { get; set; }
}