using System.ComponentModel.DataAnnotations;

namespace SocialMediaAssistant.API.Dtos
{
    public class CreateUserDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Şimdilik şifreyi basit tutuyoruz, daha sonra hashing ekleyeceğiz.
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}