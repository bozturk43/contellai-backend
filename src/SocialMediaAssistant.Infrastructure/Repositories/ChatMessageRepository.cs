using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;

namespace SocialMediaAssistant.Infrastructure.Repositories;

public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{

    public ChatMessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(Guid userId)
    {
        return await _context.ChatMessages
            .Where(cm => cm.UserId == userId)
            .OrderBy(cm => cm.Timestamp)
            .ToListAsync();
    }
    public async Task<int> DeleteByUserIdAsync(Guid userId)
    {
        // EF Core 7+ ile gelen ExecuteDeleteAsync, verileri belleğe çekmeden,
        // doğrudan veritabanında silme işlemi yapar.
        return await _context.ChatMessages
            .Where(cm => cm.UserId == userId)
            .ExecuteDeleteAsync();
    }
}