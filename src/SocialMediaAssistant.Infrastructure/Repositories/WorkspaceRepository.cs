using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


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
            .Include(w => w.User) // İLİŞKİLİ USER NESNESİNİ DAHİL ET
            .FirstOrDefaultAsync(w => w.Id == id);
    }
}   