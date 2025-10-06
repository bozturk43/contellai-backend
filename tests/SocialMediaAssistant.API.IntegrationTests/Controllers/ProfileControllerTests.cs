using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using SocialMediaAssistant.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using SocialMediaAssistant.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using SocialMediaAssistant.API.Dtos; // UserProfileDto için eklendi

namespace SocialMediaAssistant.API.IntegrationTests.Controllers;

public class ProfileControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProfileControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Bu yardımcı metodu diğer test dosyasından kopyalıyoruz.
    // Proje büyüdüğünde bu tür yardımcı metotlar ortak bir dosyaya taşınabilir.
    private string GenerateTestJwtToken(User user, IConfiguration config)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task GetMyProfile_WhenUserIsAuthenticated_ShouldReturnCorrectProfileData()
    {
        // ARRANGE (Hazırlık)
        // 1. Test için belirli bilgilere sahip bir kullanıcı oluşturuyoruz.
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "profile-test@example.com",
            Name = "Profile Test User",
            CoinBalance = 150
        };

        // 2. Bu kullanıcıyı hafıza-içi veritabanına ekliyoruz.
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();

            // 3. Bu kullanıcı için bir JWT token üretip HttpClient'a ekliyoruz.
            var token = GenerateTestJwtToken(testUser, config);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // ACT (Eylem)
        // 4. GET /api/me endpoint'ine istek atıyoruz.
        var response = await _client.GetAsync("/api/me");

        // ASSERT (Doğrulama)
        // 5. Cevabın başarılı (200 OK) olduğunu doğruluyoruz.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Dönen JSON cevabını UserProfileDto nesnesine dönüştürüyoruz.
        var profileDto = await response.Content.ReadFromJsonAsync<UserProfileDto>();

        // 7. Dönen bilgilerin, başta oluşturduğumuz test kullanıcısının bilgileriyle eşleştiğini,
        //    ve PasswordHash gibi hassas bilgileri İÇERMEDİĞİNİ doğruluyoruz.
        profileDto.Should().NotBeNull();
        profileDto.Id.Should().Be(testUser.Id);
        profileDto.Name.Should().Be(testUser.Name);
        profileDto.Email.Should().Be(testUser.Email);
        profileDto.CoinBalance.Should().Be(testUser.CoinBalance);
    }
}