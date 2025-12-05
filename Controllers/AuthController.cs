using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.username) || string.IsNullOrWhiteSpace(model.password))
                return BadRequest(new { message = "Username and password are required." });

            // find user by username
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == model.username);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            // NOTE: project currently uses User.OTP as a credential placeholder.
            // Replace with proper password hashing & verification in production.
            if (string.IsNullOrEmpty(user.OTP) || user.OTP != model.password)
                return Unauthorized(new { message = "Invalid username or password." });

            string token;
            try
            {
                token = GenerateJwtToken(user);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

            var response = new LoginResponse
            {
                token = token,
                message = "Login successful",
                user = new UserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    LinkAvatar = user.LinkAvatar,
                    RoleName = user.Role?.Name ?? user.Role?.RoleName
                }
            };

            return Ok(response);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Missing configuration 'Jwt:Key'.");
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("Missing configuration 'Jwt:Issuer' or 'Jwt:Audience'.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("role", user.Role?.Name ?? user.Role?.RoleName ?? "User")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // DTOs used by this controller
        public class LoginRequest
        {
            public string username { get; set; } = default!;
            public string password { get; set; } = default!;
        }

        public class LoginResponse
        {
            public string token { get; set; } = default!;
            public string message { get; set; } = default!;
            public UserDto? user { get; set; }
        }

        public class UserDto
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = default!;
            public string? Email { get; set; }
            public string? LinkAvatar { get; set; }
            public string? RoleName { get; set; }
        }
    }
}