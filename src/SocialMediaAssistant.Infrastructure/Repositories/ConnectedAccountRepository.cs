using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;

namespace SocialMediaAssistant.Infrastructure.Repositories;

public class ConnectedAccountRepository : Repository<ConnectedAccount>, IConnectedAccountRepository
{
    public ConnectedAccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ConnectedAccount>> GetAccountsByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.ConnectedAccounts
                             .Where(acc => acc.WorkspaceId == workspaceId)
                             .ToListAsync();
    }
}