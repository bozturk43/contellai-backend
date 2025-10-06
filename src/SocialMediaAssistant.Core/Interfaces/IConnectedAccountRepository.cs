using SocialMediaAssistant.Core.Entities;

namespace SocialMediaAssistant.Core.Interfaces;

public interface IConnectedAccountRepository : IRepository<ConnectedAccount>
{
    Task<IEnumerable<ConnectedAccount>> GetAccountsByWorkspaceIdAsync(Guid workspaceId);
}