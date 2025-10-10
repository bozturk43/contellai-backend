using Pgvector; // Artık bu 'using' ifadesi hata vermeyecek

namespace SocialMediaAssistant.Core.Entities;

public class KnowledgeArticle
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
}