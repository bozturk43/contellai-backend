using SocialMediaAssistant.Application.Interfaces;
using SocialMediaAssistant.Core.Enums;
using SocialMediaAssistant.Core.Interfaces;

namespace SocialMediaAssistant.Application.Services;

public class CoinService : ICoinService
{
    private readonly IUnitOfWork _unitOfWork;

    public CoinService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private int GetCostForContentType(ContentType contentType)
    {
        return contentType switch
        {
            ContentType.Post => 15,
            ContentType.Story => 20,
            ContentType.Reels => 25,
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), "Geçersiz içerik türü.")
        };
    }

    public async Task<bool> HasSufficientCoins(Guid userId, ContentType contentType)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        var cost = GetCostForContentType(contentType);
        return user.CoinBalance >= cost;
    }

    public async Task DeductCoinsForAction(Guid userId, ContentType contentType)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) 
            throw new InvalidOperationException("Kullanıcı bulunamadı.");

        var cost = GetCostForContentType(contentType);
        if (user.CoinBalance < cost)
            throw new InvalidOperationException("Yetersiz bakiye.");

        user.CoinBalance -= cost;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync(); // Değişikliği anında veritabanına kaydet
    }
    public async Task AddCoinsAsync(Guid userId, CoinPackageType packageType)
    {
        var amount = GetAmountForPackageType(packageType);

        if (amount <= 0)
        {
            throw new ArgumentException("Geçersiz paket türü.", nameof(packageType));
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("Kredi eklenecek kullanıcı bulunamadı.");
        }

        user.CoinBalance += amount;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();
    }
    private int GetAmountForPackageType(CoinPackageType packageType)
    {
        return packageType switch
        {
            CoinPackageType.Basic => 100,
            CoinPackageType.Standard => 500,
            CoinPackageType.Premium => 1500,
            _ => 0
        };
    }
}