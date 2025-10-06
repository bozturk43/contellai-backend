using SocialMediaAssistant.Core.Enums;
using SocialMediaAssistant.Core.Interfaces;

namespace SocialMediaAssistant.API.BackgroundServices;

public class PublishingService : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly ILogger<PublishingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public PublishingService(ILogger<PublishingService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Zamanlanmış İçerik Yayınlama Servisi Başlatılıyor.");
        // Her 1 dakikada bir DoWork metodunu çalıştıracak bir zamanlayıcı kuruyoruz.
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Zamanlanmış içerikler kontrol ediliyor...");

        // Arka plan servisleri "singleton" gibi çalıştığı için, her seferinde
        // "scoped" olan DbContext ve UnitOfWork'e erişmek için yeni bir scope oluşturmalıyız.
        using (var scope = _scopeFactory.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            // Yayınlanma zamanı gelmiş içerikleri bul
            var postsToPublish = unitOfWork.ContentPosts.GetScheduledPostsToPublishAsync().Result;

            if (!postsToPublish.Any())
            {
                _logger.LogInformation("Yayınlanacak yeni içerik bulunamadı.");
                return;
            }

            foreach (var post in postsToPublish)
            {
                post.Status = PostStatus.Published;
                post.PublishedAt = DateTime.UtcNow;
                unitOfWork.ContentPosts.Update(post);
                _logger.LogInformation($"İçerik ID {post.Id} yayınlandı olarak işaretlendi.");
            }

            // Tüm değişiklikleri veritabanına kaydet
            unitOfWork.CompleteAsync().Wait();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yayınlama Servisi Durduruluyor.");
        _timer?.Change(Timeout.Infinite, 0); // Zamanlayıcıyı durdur
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}