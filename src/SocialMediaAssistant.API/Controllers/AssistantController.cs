using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Core.Entities;
using Pgvector;
using System.IO;
using System.Text.Json;
using System.Security.Claims;
using SocialMediaAssistant.Core.Enums;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssistantController : ControllerBase
{
    private readonly GoogleAI _googleAI;
    private readonly IUnitOfWork _unitOfWork;

    public AssistantController(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        var apiKey = configuration["Google:ApiKey"] ?? throw new InvalidOperationException("Google API anahtarı bulunamadı.");
        _googleAI = new GoogleAI(apiKey);
        _unitOfWork = unitOfWork;
    }

    // === CHAT İŞLEMLERİ İÇİN DTO'LAR ===
    public class ChatMessageDto
    {
        public string Sender { get; set; } = "user"; // "user" veya "ai"
        public string Text { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        public List<ChatMessageDto> History { get; set; } = new();
        public string NewMessage { get; set; } = string.Empty;
    }
    // === ANA CHAT ENDPOINT'İ ===
    [HttpPost("chat")]
    public async Task<IActionResult> PostChatMessage([FromBody] ChatRequest request)
    {
        var userId = GetCurrentUserId();

        // 1. ADIM (Retrieval): Kullanıcının yeni sorusunu vektöre çevir
        var questionEmbedding = new Vector(await GetEmbeddingAsync(request.NewMessage));
         // 2. ADIM (Retrieval): Veritabanında bu soruya en uygun bilgi makalesini ara
        var relevantArticle = await _unitOfWork.KnowledgeArticles.FindMostRelevantArticleAsync(questionEmbedding);
        var model = _googleAI.GenerativeModel(model: Model.GeminiPro);
        
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Sen, ContellAI uygulaması için çalışan, adı 'Connie' olan dost canlısı ve yardımcı bir asistansın.");
        promptBuilder.AppendLine("Görevin, kullanıcılara hem uygulamayı nasıl kullanacakları konusunda yardımcı olmak hem de genel sosyal medya stratejileri hakkında ipuçları vermektir.");
        if (relevantArticle != null)
        {
            promptBuilder.AppendLine("---");
            promptBuilder.AppendLine("Kullanıcının sorusunu cevaplarken ÖNCELİKLE aşağıdaki bilgiyi kullan. Bu bilgi, uygulamanın kendi dokümantasyonundan alınmıştır:");
            promptBuilder.AppendLine($"İlgili Doküman: {relevantArticle.Content}");
        }
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine("Konuşma Geçmişi:");
        foreach (var message in request.History)
        {
            var sender = message.Sender == "user" ? "Kullanıcı" : "Asistan";
            promptBuilder.AppendLine($"{sender}: {message.Text}");
        }
        
        promptBuilder.AppendLine($"Yeni Soru: {request.NewMessage}");
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine("Cevabın:");

        var response = await model.GenerateContent(promptBuilder.ToString());
        
        // TODO: Kullanıcının sorusunu ve AI'ın cevabını ChatMessages tablosuna kaydet.
        var aiReplyText = response.Text;

        var userMessage = new SocialMediaAssistant.Core.Entities.ChatMessage
        {
            UserId = userId,
            Sender = MessageSender.User,
            Text = request.NewMessage
        };
        var aiMessage = new SocialMediaAssistant.Core.Entities.ChatMessage
        {
            UserId = userId,
            Sender = MessageSender.Ai,
            Text = aiReplyText
        };

        await _unitOfWork.ChatMessages.AddAsync(userMessage);
        await _unitOfWork.ChatMessages.AddAsync(aiMessage);
        await _unitOfWork.CompleteAsync();

        return Ok(new { reply = aiReplyText });
    }
    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory()
    {
        var userId = GetCurrentUserId();
        var history = await _unitOfWork.ChatMessages.GetMessagesByUserIdAsync(userId);
        var historyDto = history.Select(m => new ChatMessageDto 
        { 
            Sender = m.Sender.ToString().ToLower(),
            Text = m.Text,
        });

        return Ok(historyDto);
    }
    [HttpDelete("history")]
    public async Task<IActionResult> DeleteHistory()
    {
        var userId = GetCurrentUserId();

        await _unitOfWork.ChatMessages.DeleteByUserIdAsync(userId);
        // CompleteAsync'e gerek yok, ExecuteDeleteAsync doğrudan veritabanını etkiler.

        return NoContent();
    }
    // === BİLGİ BANKASINI DOLDURMA (SEEDING) İŞLEMLERİ ===
    public class KnowledgeSeedData
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
    // Metni, Google'ın Embedding Modeli ile vektöre çeviren yardımcı metot
    private async Task<float[]> GetEmbeddingAsync(string text)
    {
         var model = _googleAI.GenerativeModel(model: "text-embedding-004");
        var response = await model.EmbedContent(text);
        return response.Embedding.Values.ToArray();
    }
    // Sadece 1 kez çağıracağımız, bilgi bankasını dolduran endpoint
    [HttpPost("seed-knowledge")]
    public async Task<IActionResult> SeedKnowledge()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "knowledge.json");
        if (!System.IO.File.Exists(jsonPath))
        {
            return NotFound("knowledge.json dosyası bulunamadı.");
        }
        var jsonContent = await System.IO.File.ReadAllTextAsync(jsonPath);
        var articlesFromFile = JsonSerializer.Deserialize<List<KnowledgeSeedData>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (articlesFromFile == null || !articlesFromFile.Any())
        {
            return BadRequest("JSON dosyası boş veya hatalı formatta.");
        }

        // Veritabanındaki mevcut tüm makalelerin başlıklarını bir listeye al (verimli kontrol için)
        var existingTitles = (await _unitOfWork.KnowledgeArticles.GetAllAsync()).Select(a => a.Title).ToHashSet();
        
        int newArticlesCount = 0;
        foreach (var articleData in articlesFromFile)
        {
            // Eğer bu başlıktaki bir makale zaten veritabanında varsa, bu adımı atla (Idempotency)
            if (existingTitles.Contains(articleData.Title))
            {
                continue; // Bu makale zaten var, sonrakine geç
            }

            // Yeni makale için işlemleri yap
            var newArticle = new KnowledgeArticle
            {
                Title = articleData.Title,
                Content = articleData.Content
            };
            
            var embedding = await GetEmbeddingAsync(newArticle.Content);
            newArticle.Embedding = new Vector(embedding);
            
            await _unitOfWork.KnowledgeArticles.AddAsync(newArticle);
            newArticlesCount++;
        }
        
        // Eğer yeni makale eklendiyse, değişiklikleri veritabanına kaydet
        if (newArticlesCount > 0)
        {
            await _unitOfWork.CompleteAsync();
        }

        return Ok($"{newArticlesCount} adet yeni bilgi parçacığı veritabanına eklendi. {existingTitles.Count} adet zaten mevcuttu.");
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdString!);
    }
}