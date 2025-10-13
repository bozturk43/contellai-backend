    using SocialMediaAssistant.Core.Entities;

    namespace SocialMediaAssistant.Core.Interfaces;

    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(Guid userId);
        Task<int> DeleteByUserIdAsync(Guid userId);
    }