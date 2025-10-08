namespace SocialMediaAssistant.Core.Entities;

public class Workspace
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string BrandTone { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User? User { get; set; }
    public ICollection<ContentPost> ContentPosts { get; set; } = new List<ContentPost>();
    public ICollection<ConnectedAccount> ConnectedAccounts { get; set; } = new List<ConnectedAccount>();


}