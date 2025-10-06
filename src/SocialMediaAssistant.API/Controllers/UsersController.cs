using Microsoft.AspNetCore.Mvc;
using SocialMediaAssistant.API.Dtos;
using SocialMediaAssistant.Core.Entities; 
using SocialMediaAssistant.Core.Interfaces;
using BCrypt.Net;


namespace SocialMediaAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                Email = u.Email
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
            var newUser = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = global::BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                CoinBalance = 10,
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync();

            // Oluşturulan kaynağın adresini ve kendisini 201 Created cevabıyla döndür
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
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
    }
}