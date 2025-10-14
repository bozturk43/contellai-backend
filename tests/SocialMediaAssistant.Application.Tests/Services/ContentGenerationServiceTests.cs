using Xunit;
using Moq;
using FluentAssertions;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Application.Services;
using SocialMediaAssistant.Application.Interfaces;
using System.Threading.Tasks;
using SocialMediaAssistant.Application.Models;


namespace SocialMediaAssistant.Application.Tests.Services;


public class ContentGenerationServiceTests
{
    private readonly Mock<IGeminiService> _geminiServiceMock;
    private readonly Mock<IImageGenerationService> _imageGenerationServiceMock;
    private readonly ContentGenerationService _contentGenerationService;

    public ContentGenerationServiceTests()
    {
        // Her test için sahte servislerimizi (mock) oluşturuyoruz.
        _geminiServiceMock = new Mock<IGeminiService>();
        _imageGenerationServiceMock = new Mock<IImageGenerationService>();
        
        // Asıl test edeceğimiz servisi, bu sahte servislerle başlatıyoruz.
        _contentGenerationService = new ContentGenerationService( _imageGenerationServiceMock.Object,_geminiServiceMock.Object);
    }

    [Fact]
    public async Task GenerateContentAsync_ShouldOrchestrateAiCallsCorrectly()
    {
        // ARRANGE (Hazırlık): Test için gerekli verileri ve sahte servislerin davranışlarını ayarla

        // 1. Girdi verilerini tanımla
        var userPrompt = "sahilde gün batımı";
        var context = new Workspace { BrandName = "Tatil Köyü", Industry = "Turizm" };
        
        // 2. Servislerin döndürmesini beklediğimiz sahte sonuçları tanımla
        var expectedImagePrompt = "A cinematic, photorealistic image of a sunset on the beach";
        var expectedSocialText = "Sahilde harika bir gün batımı... 🌅 #tatil";
        var expectedImageUrl = "http://example.com/sunset.jpg";

        // 3. Sahte Gemini servisimize talimatlar ver:
        //    - Eğer sana içinde "İngilizce bir prompt oluştur" geçen bir metin gelirse, 'expectedImagePrompt'u döndür.
        _geminiServiceMock
            .Setup(s => s.GenerateTextAsync(It.Is<string>(p => p.Contains("İngilizce bir prompt oluştur"))))
            .ReturnsAsync(expectedImagePrompt);
        
        //    - Eğer sana içinde "sosyal medya içerik üretme asistanısın" geçen bir metin gelirse, 'expectedSocialText'i döndür.
        _geminiServiceMock
            .Setup(s => s.GenerateTextAsync(It.Is<string>(p => p.Contains("sosyal medya içerik üretme asistanısın"))))
            .ReturnsAsync(expectedSocialText);

        // 4. Sahte Resim servisimize talimat ver:
        //    - Eğer sana 'expectedImagePrompt' metni ile gelinirse, 'expectedImageUrl'ü döndür.
        _imageGenerationServiceMock
            .Setup(s => s.GenerateImageUrlAsync(expectedImagePrompt))
            .ReturnsAsync(expectedImageUrl);

        // ACT (Eylem): Test etmek istediğimiz asıl metodu çağır
        var result = await _contentGenerationService.GenerateContentAsync(userPrompt, context);

        // ASSERT (Doğrulama): Sonuçların ve servis çağrılarının doğruluğunu kontrol et
        
        // 1. Dönen nihai sonucun beklediğimiz gibi olup olmadığını doğrula.
        result.GeneratedText.Should().Be(expectedSocialText);
        result.GeneratedImageUrl.Should().Be(expectedImageUrl);

        // 2. 'IImageGenerationService'in, Gemini'dan dönen DOĞRU prompt ile sadece bir kez çağrıldığını doğrula.
        //    Bu, orkestrasyonun en kritik adımıdır.
        _imageGenerationServiceMock.Verify(s => s.GenerateImageUrlAsync(expectedImagePrompt), Times.Once);
        
        // 3. 'IGeminiService'in toplamda iki kez çağrıldığını doğrula (biri metin, diğeri görsel prompt'u için).
        _geminiServiceMock.Verify(s => s.GenerateTextAsync(It.IsAny<string>()), Times.Exactly(2));
    }
}