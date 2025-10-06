using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAssistant.API.Dtos;
using SocialMediaAssistant.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialMediaAssistant.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProfileController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("me")] // Endpoint adresi: GET /api/me
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                // [Authorize] attribute'u olduğu için bu durumun yaşanması pek olası değil,
                // ama her zaman kontrol etmek iyidir.
                return Unauthorized();
            }

            // Veritabanı nesnesini (User) DTO'ya dönüştür
            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CoinBalance = user.CoinBalance
            };

            return Ok(profileDto);
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdString!);
        }
    }
}