using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Core.Entities;
using Pgvector;

namespace SocialMediaAssistant.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<ConnectedAccount> ConnectedAccounts { get; set; }
    public DbSet<ContentPost> ContentPosts { get; set; }
    public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 'vector' uzantısını EF Core'a tanıtıyoruz.
        modelBuilder.HasPostgresExtension("vector");
    }

}