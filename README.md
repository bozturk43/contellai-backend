# ContellAI Backend

Bu repo, **ContellAI** projesinin backend servislerini içerir. ContellAI, yapay zeka destekli bir sosyal medya içerik üretme asistanıdır. Bu servis, kullanıcı yönetimi, kimlik doğrulama, çalışma alanı yönetimi ve Google Gemini API'si ile dinamik içerik üretme gibi temel işlevleri barındırır.

## 🚀 Projenin Amacı

Küçük ve orta ölçekli işletmelerin veya bireysel kullanıcıların, marka kimliklerine uygun, yaratıcı ve etkili sosyal medya içeriklerini (metin ve görsel) kolayca üretmelerini sağlamak.

## 🛠️ Kullanılan Teknolojiler ve Mimariler

- **Platform:** .NET 8
- **Mimari:** Katmanlı Mimari (Core, Infrastructure, Application, API)
- **Veritabanı:** PostgreSQL
- **ORM:** Entity Framework Core
- **API:** RESTful API (.NET Web API)
- **Kimlik Doğrulama:** JWT (JSON Web Tokens)
- **Tasarım Desenleri:**
  - Repository & Unit of Work
  - Dependency Injection (DI)
- **Test:** xUnit, Moq, FluentAssertions (Birim ve Entegrasyon Testleri)
- **Yapay Zeka:** Google Gemini API

## 🏁 Projeyi Yerel Makinede Çalıştırma

Projeyi kendi bilgisayarınızda çalıştırmak için aşağıdaki adımları izleyin.

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PostgreSQL Veritabanı
- Git

### Kurulum Adımları

1.  **Repoyu Klonlama:**
    ```bash
    git clone [https://github.com/bozturk43/contellai-backend.git](https://github.com/bozturk43/contellai-backend.git)
    cd contellai-backend
    ```

2.  **Gizli Anahtarları (User Secrets) Yapılandırma:**
    Projenin `API` katmanı için gizli anahtarları yapılandırmanız gerekmektedir.

    ```bash
    # Secret Manager'ı başlat
    dotnet user-secrets init --project src/SocialMediaAssistant.API

    # Veritabanı Bağlantı Cümlenizi Ekleyin
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "SENIN_POSTGRESQL_CONNECTION_STRING"

    # JWT Gizli Anahtarınızı Ekleyin
    dotnet user-secrets set "Jwt:Key" "COK_GIZLI_VE_GUCLU_BIR_ANAHTAR_GIRINIZ"

    # Google Gemini API Anahtarınızı Ekleyin
    dotnet user-secrets set "Google:ApiKey" "SENIN_GOOGLE_GEMINI_API_ANAHTARIN"
    
    # Unsplash API Anahtarınızı Ekleyin
    dotnet user-secrets set "Unsplash:AccessKey" "SENIN_UNSPLASH_ACCESS_KEY"
    ```
    *Not: `Jwt:Issuer` ve `Jwt:Audience` değerleri `appsettings.json` dosyasında bırakılmıştır.*

3.  **Veritabanını Oluşturma (Migrations):**
    ```bash
    dotnet ef database update --project src/SocialMediaAssistant.Infrastructure --startup-project src/SocialMediaAssistant.API
    ```

4.  **Uygulamayı Çalıştırma:**
    ```bash
    dotnet run --project src/SocialMediaAssistant.API
    ```
    Uygulama varsayılan olarak `https://localhost:XXXX` ve `http://localhost:YYYY` portlarında çalışmaya başlayacaktır. Swagger arayüzüne `https://localhost:XXXX/swagger` adresinden ulaşabilirsiniz.

## ✅ Testleri Çalıştırma

Projedeki tüm birim ve entegrasyon testlerini çalıştırmak için aşağıdaki komutu kullanın:

```bash
dotnet test
```

---