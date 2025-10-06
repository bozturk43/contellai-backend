using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Core.Entities;

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
}