using SocialMediaAssistant.Core.Enums;

public class ConnectedAccountDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public PlatformType Platform { get; set; }
    public string PlatformUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}