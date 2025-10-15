using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Core.Models;


namespace SocialMediaAssistant.Infrastructure.Repositories;

public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(ApplicationDbContext context) : base(context)
    {
    }
    public override async Task<IEnumerable<Workspace>> GetAllAsync()
    {
        return await _context.Workspaces
            .Include(w => w.User)
            .ToListAsync();
    }
    public override async Task<Workspace?> GetByIdAsync(Guid id)
    {
        // Belirli bir workspace'i getirirken, ona ait User nesnesini de dahil et.
        // Not: .Include() metodu FindAsync ile çalışmaz, bu yüzden FirstOrDefaultAsync kullanıyoruz.
        return await _context.Workspaces
        .Include(w => w.User)
        .Include(w => w.ConnectedAccounts)
        .FirstOrDefaultAsync(w => w.Id == id);
    }
    public async Task<IEnumerable<Workspace>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Workspaces
        .Where(w => w.UserId == userId)
        .Include(w => w.ContentPosts)
        .Include(w => w.ConnectedAccounts)
        .ToListAsync();
    }
    public async Task<IEnumerable<WorkspaceSummary>> GetSummariesByUserIdAsync(Guid userId)
    {
        return await _context.Workspaces
            .Where(w => w.UserId == userId)
            .Select(w => new WorkspaceSummary // DTO yerine yeni 'Summary' modeline projeksiyon
            {
                Id = w.Id,
                BrandName = w.BrandName,
                Industry = w.Industry,
                PostCount = w.ContentPosts.Count(),
                AccountCount = w.ConnectedAccounts.Count()
            })
            .ToListAsync();
    }
}   