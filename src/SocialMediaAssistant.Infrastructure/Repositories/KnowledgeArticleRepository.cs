using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; 




namespace SocialMediaAssistant.Infrastructure.Repositories;

public class KnowledgeArticleRepository : Repository<KnowledgeArticle>, IKnowledgeArticleRepository
{
    public KnowledgeArticleRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<KnowledgeArticle?> FindMostRelevantArticleAsync(Vector embedding)
    {
        // Veritabanındaki tüm makaleleri, bizim arama vektörümüze
        // anlamsal olarak ne kadar benzediğine göre sırala (CosineDistance).
        // En çok benzeyen (mesafesi en küçük olan) ilk sonucu al.
        return await _context.KnowledgeArticles
            .OrderBy(x => x.Embedding!.CosineDistance(embedding))
            .FirstOrDefaultAsync();
    }
}