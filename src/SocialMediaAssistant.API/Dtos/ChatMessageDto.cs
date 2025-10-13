namespace SocialMediaAssistant.API.Dtos
{
    public class ChatMessageDto
    {
        public string Sender { get; set; } = string.Empty; // "user" veya "ai"
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}