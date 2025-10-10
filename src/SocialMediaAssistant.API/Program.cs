using Microsoft.EntityFrameworkCore;
using SocialMediaAssistant.Infrastructure.Data;
using SocialMediaAssistant.Core.Interfaces;
using SocialMediaAssistant.Infrastructure.Repositories;
using SocialMediaAssistant.Application.Interfaces;
using SocialMediaAssistant.Application.Services;
using SocialMediaAssistant.API.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Pgvector.Npgsql;


var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var allowedOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          if (allowedOrigins != null && allowedOrigins.Length > 0)
                          {
                              policy.WithOrigins(allowedOrigins)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          }
                      });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseVector()));
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Swagger'a bir güvenlik tanımı (security definition) ekliyoruz.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Lütfen 'Bearer' yazıp bir boşluk bıraktıktan sonra token'ı girin.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Swagger'a, yetkilendirme gerektiren tüm endpoint'ler için
    // yukarıda tanımladığımız güvenlik şemasını kullanmasını söylüyoruz.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IContentGenerationService, ContentGenerationService>();
builder.Services.AddScoped<ICoinService, CoinService>();
builder.Services.AddHostedService<PublishingService>();
// builder.Services.AddHttpClient<IImageGenerationService, UnsplashImageGenerationService>();
builder.Services.AddHttpClient<IImageGenerationService, HuggingFaceImageGenerationService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddControllers();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
