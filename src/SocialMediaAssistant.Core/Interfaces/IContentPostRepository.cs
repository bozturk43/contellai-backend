using SocialMediaAssistant.Core.Entities;

namespace SocialMediaAssistant.Core.Interfaces;

public interface IContentPostRepository : IRepository<ContentPost>
{
    Task<IEnumerable<ContentPost>> GetPostsByWorkspaceIdAsync(Guid workspaceId);
    Task<IEnumerable<ContentPost>> GetScheduledPostsToPublishAsync();

}
