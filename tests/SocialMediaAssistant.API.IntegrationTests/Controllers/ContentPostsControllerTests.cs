using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Enums;
using SocialMediaAssistant.API.Dtos;
using Microsoft.Extensions.DependencyInjection;
using SocialMediaAssistant.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.Json; // JsonSerializer için eklendi

namespace SocialMediaAssistant.API.IntegrationTests.Controllers;

public class ContentPostsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ContentPostsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

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
    public async Task GeneratePreview_WhenUserHasNoCoins_ShouldReturnBadRequest()
    {
        // ARRANGE
        var userWithNoCoins = new User { Id = Guid.NewGuid(), Email = "no-coins@test.com", Name = "Test", CoinBalance = 0 };
        var workspace = new Workspace { Id = Guid.NewGuid(), BrandName = "Test Brand", UserId = userWithNoCoins.Id };

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            
            await context.Users.AddAsync(userWithNoCoins);
            await context.Workspaces.AddAsync(workspace);
            await context.SaveChangesAsync();

            var token = GenerateTestJwtToken(userWithNoCoins, config);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var contentToCreate = new CreateContentPostDto
        {
            WorkspaceId = workspace.Id,
            UserPrompt = "Bu bir test prompt'udur.",
            ContentType = ContentType.Post
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/contentposts/generate-preview", contentToCreate);

        // ASSERT
        // 1. Durum kodunun 400 Bad Request olduğunu doğrula
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 2. DÜZELTME: Dönen cevabı string olarak oku ve JSON'dan 'message' alanını çekerek kontrol et
        var responseString = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
        
        // Hata mesajının tam olarak beklediğimiz gibi olduğunu doğrula
        errorResponse.GetProperty("message").GetString().Should().Be("Bu içerik türü için yeterli krediniz (coin) bulunmamaktadır.");
    }
}