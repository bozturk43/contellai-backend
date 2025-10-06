using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;

namespace SocialMediaAssistant.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserRepository Users { get; private set; }
        public IWorkspaceRepository Workspaces { get; private set; }
        public IConnectedAccountRepository ConnectedAccounts { get; private set; }
        public IContentPostRepository ContentPosts { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Workspaces = new WorkspaceRepository(_context);
            ConnectedAccounts = new ConnectedAccountRepository(_context);
            ContentPosts = new ContentPostRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}