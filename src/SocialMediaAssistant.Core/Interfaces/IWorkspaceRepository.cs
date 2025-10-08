using SocialMediaAssistant.Core.Entities;

namespace SocialMediaAssistant.Core.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace>
{
        Task<IEnumerable<Workspace>> GetByUserIdAsync(Guid userId);
}