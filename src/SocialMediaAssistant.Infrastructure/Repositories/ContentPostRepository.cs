using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;
using SocialMediaAssistant.Core.Enums;

namespace SocialMediaAssistant.Infrastructure.Repositories;

public class ContentPostRepository : Repository<ContentPost>, IContentPostRepository
{
    public ContentPostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ContentPost>> GetPostsByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.ContentPosts.Where(p => p.WorkspaceId == workspaceId)
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
    }
    public async Task<IEnumerable<ContentPost>> GetScheduledPostsToPublishAsync()
    {
        return await _context.ContentPosts
            .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledAt <= DateTime.UtcNow)
            .ToListAsync();
    }
}