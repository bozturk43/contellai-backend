using SocialMediaAssistant.Core.Enums;

namespace SocialMediaAssistant.Application.Interfaces;

public interface ICoinService
{
    Task<bool> HasSufficientCoins(Guid userId, ContentType contentType);
    Task DeductCoinsForAction(Guid userId, ContentType contentType);
    Task AddCoinsAsync(Guid userId, CoinPackageType packageType);

}