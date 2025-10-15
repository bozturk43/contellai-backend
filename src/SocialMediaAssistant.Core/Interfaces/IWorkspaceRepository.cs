using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Models;

namespace SocialMediaAssistant.Core.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace>
{
        Task<IEnumerable<Workspace>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<WorkspaceSummary>> GetSummariesByUserIdAsync(Guid userId); // YENİ METOT

}