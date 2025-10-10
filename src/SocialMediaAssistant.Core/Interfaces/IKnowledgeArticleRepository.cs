using SocialMediaAssistant.Core.Entities;
using Pgvector;

namespace SocialMediaAssistant.Core.Interfaces;

public interface IKnowledgeArticleRepository : IRepository<KnowledgeArticle>
{
    Task<KnowledgeArticle?> FindMostRelevantArticleAsync(Vector embedding);
}