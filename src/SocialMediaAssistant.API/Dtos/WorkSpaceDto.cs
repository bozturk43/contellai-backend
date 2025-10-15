namespace SocialMediaAssistant.API.Dtos
{
    public class WorkspaceDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty;
        public string BrandTone { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // DİKKAT: User entity'si yerine UserDto kullanıyoruz
        public UserDto? User { get; set; }
        public List<ConnectedAccountDto> ConnectedAccounts { get; set; } = new();
        public List<ContentPostDto> ContentPosts { get; set; } = new();
        public int PostCount { get; set; }
        public int AccountCount { get; set; }

    }
}