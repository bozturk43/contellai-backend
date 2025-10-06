using Xunit;
using Moq;
using FluentAssertions;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Enums;
using SocialMediaAssistant.Application.Services;

namespace SocialMediaAssistant.Application.Tests.Services;

public class CoinServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CoinService _coinService;

    public CoinServiceTests()
    {
        // Her testten önce, IUnitOfWork'ün sahte (mock) bir versiyonu.
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        // CoinService'i, bu sahte IUnitOfWork ile başlat.
        // Artık servisimiz gerçek veritabanıyla konuşmayacak.
        _coinService = new CoinService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HasSufficientCoins_ShouldReturnTrue_WhenUserHasEnoughCoins()
    {
        // ARRANGE (Hazırlık)
        // 1. Test için sahte verileri oluşturuyoruz.
        var testUser = new User { Id = Guid.NewGuid(), CoinBalance = 100 };
        var contentType = ContentType.Post; // Post'un maliyeti 15

        // 2. Mock'umuza talimat veriyoruz: "IUnitOfWork.Users.GetByIdAsync metodu
        //    bu testUser'ın Id'si ile çağrıldığında, geriye bu testUser nesnesini döndür."
        _unitOfWorkMock.Setup(uow => uow.Users.GetByIdAsync(testUser.Id)).ReturnsAsync(testUser);

        // ACT (Eylem)
        // 3. Test etmek istediğimiz metodu çağırıyoruz.
        var result = await _coinService.HasSufficientCoins(testUser.Id, contentType);

        // ASSERT (Doğrulama)
        // 4. Sonucun beklediğimiz gibi olup olmadığını kontrol ediyoruz.
        //    Kullanıcının 100 kredisi var, Post 15 kredi, yani sonuç 'true' olmalı.
        result.Should().BeTrue();
    }
    [Fact]
    public async Task HasSufficientCoins_ShouldReturnFalse_WhenUserHasNotEnoughCoins()
    {
        // ARRANGE
        // Kullanıcının sadece 10 kredisi var, ama Post'un maliyeti 15.
        var testUser = new User { Id = Guid.NewGuid(), CoinBalance = 10 };
        var contentType = ContentType.Post; // Maliyet: 15
        _unitOfWorkMock.Setup(uow => uow.Users.GetByIdAsync(testUser.Id)).ReturnsAsync(testUser);

        // ACT
        var result = await _coinService.HasSufficientCoins(testUser.Id, contentType);

        // ASSERT
        // Sonucun 'false' olmasını bekliyoruz.
        result.Should().BeFalse();
    }
    [Fact]
    public async Task DeductCoinsForAction_ShouldDecreaseCoinBalanceByCorrectAmount()
    {
        // ARRANGE
        var initialBalance = 100;
        var costOfReels = 25; // Reels'in maliyeti
        var expectedBalance = initialBalance - costOfReels; // Beklenen bakiye: 75
        var testUser = new User { Id = Guid.NewGuid(), CoinBalance = initialBalance };
        var contentType = ContentType.Reels;
        _unitOfWorkMock.Setup(uow => uow.Users.GetByIdAsync(testUser.Id)).ReturnsAsync(testUser);

        // ACT
        await _coinService.DeductCoinsForAction(testUser.Id, contentType);

        // ASSERT
        // 1. Kredi düşüldükten sonra kullanıcının bakiyesinin beklendiği gibi olup olmadığını kontrol et.
        testUser.CoinBalance.Should().Be(expectedBalance);

        // 2. IUnitOfWork.Users.Update metodunun bu kullanıcı ile çağrıldığından emin ol.
        _unitOfWorkMock.Verify(uow => uow.Users.Update(testUser), Times.Once);

        // 3. IUnitOfWork.CompleteAsync metodunun en az bir kez çağrıldığından emin ol.
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
    [Fact]
    public async Task AddCoinsAsync_ShouldIncreaseCoinBalanceByCorrectAmount()
    {
        // ARRANGE
        var initialBalance = 100;
        var packageType = CoinPackageType.Standard; // Değeri: 500
        var expectedBalance = initialBalance + 500;
        var testUser = new User { Id = Guid.NewGuid(), CoinBalance = initialBalance };
        _unitOfWorkMock.Setup(uow => uow.Users.GetByIdAsync(testUser.Id)).ReturnsAsync(testUser);

        // ACT
        await _coinService.AddCoinsAsync(testUser.Id, packageType);

        // ASSERT
        testUser.CoinBalance.Should().Be(expectedBalance);
        _unitOfWorkMock.Verify(uow => uow.Users.Update(testUser), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
}