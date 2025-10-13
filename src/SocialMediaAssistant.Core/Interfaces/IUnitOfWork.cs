namespace SocialMediaAssistant.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWorkspaceRepository Workspaces { get; }
        IConnectedAccountRepository ConnectedAccounts { get; }
        IContentPostRepository ContentPosts { get; } 
        IKnowledgeArticleRepository KnowledgeArticles { get; }
        IChatMessageRepository ChatMessages { get; }
        Task<int> CompleteAsync();
    }
}