using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Custom;
using Products.DTOs;
using Products.Models;
using System.Security.Claims;

namespace Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Utility _utility;

        public AuthController(AppDbContext context, Utility utility)
        {
            _context = context;
            _utility = utility;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginDTO.UserName);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Password != _utility.EncryptSHA256(loginDTO.Password))
            {
                return Unauthorized();
            }

            var cookies = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            }; 
            
            Response.Cookies.Append("auth_token", _utility.GenerateJWT(user), cookies);

            return Ok( new { isSuccess = true } );
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDTO userDTO)
        {
            var user = new User
            {
                Name = userDTO.Name,
                UserName = userDTO.UserName,
                Email = userDTO.Email,
                Password = _utility.EncryptSHA256(userDTO.Password)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
            {
                return Unauthorized();
            }
            return Ok(new { withPermissions = true });
        }
    }
}
