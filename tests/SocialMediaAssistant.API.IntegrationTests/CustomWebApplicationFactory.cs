using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialMediaAssistant.Infrastructure.Data;
using System.Linq;

namespace SocialMediaAssistant.API.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Gerçek veritabanı bağlantısını (ApplicationDbContext) listeden bul ve kaldır.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // 2. Onun yerine, her test için ayrı bir isme sahip olan,
            //    hafıza-içi (in-memory) bir veritabanı sağlayıcısı ekle.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // "InMemoryDbForTesting" adı, her testin kendi izole veritabanına sahip olmasını sağlar.
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        });

        builder.UseEnvironment("Development");
    }
}