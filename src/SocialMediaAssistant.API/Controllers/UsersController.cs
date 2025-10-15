using Microsoft.AspNetCore.Mvc;
using SocialMediaAssistant.API.Dtos;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims;
using System.Text;
using SocialMediaAssistant.Application.Interfaces; 
namespace SocialMediaAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoinService _coinService;
        private readonly IConfiguration _configuration; // Token üretimi için eklendi

        public UsersController(IUnitOfWork unitOfWork, ICoinService coinService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _coinService = coinService;
            _configuration = configuration;
        }

        // GET: /api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var usersDto = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                CoinBalance = u.CoinBalance
            });
            return Ok(usersDto);
        }
        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            // Tek bir User nesnesini UserDto'ya dönüştürüyoruz
            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
            return Ok(userDto);
        }
        // POST: /api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            // Email zaten var mı diye kontrol et
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });
            }

            var newUser = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                CoinBalance = 200
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync();

            // Kullanıcıyı oluşturduktan sonra, onun için bir token üret
            var token = GenerateJwtToken(newUser.Id, newUser.Email);

            var userDtoToReturn = new UserDto
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                CoinBalance = newUser.CoinBalance
            };

            return Ok(new { token = token, user = userDtoToReturn });
        }
        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _unitOfWork.Users.Delete(user);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
        private string GenerateJwtToken(Guid userId, string email)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "User")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}