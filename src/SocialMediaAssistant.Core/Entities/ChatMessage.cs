using SocialMediaAssistant.Core.Enums;

namespace SocialMediaAssistant.Core.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } 
    public MessageSender Sender { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}