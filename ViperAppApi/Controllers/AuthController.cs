using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ViperAppApi.Data;
using ViperAppApi.Models.Domains;
using ViperAppApi.Models.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ✅ SIGNUP
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest registerRequest)
    {
        try
        {
            // Check if user already exists
            if (_context.Users.Any(u => u.Email == registerRequest.Email))
                return BadRequest(new { error = "User already exists" });

            if (_context.Users.Any(u => u.UserName == registerRequest.UserName))
                return BadRequest(new { error = "Username already taken" });

            var user = new User
            {
                UserName = registerRequest.UserName,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password)
                // CreatedAt will use database default (GETUTCDATE())
                // All other fields will be NULL (allowed by schema)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User Registered Successfully", userId = user.UserID });
        }
        catch (DbUpdateException ex)
        {
            // Get the actual SQL error
            var innerException = ex.InnerException;
            while (innerException?.InnerException != null)
                innerException = innerException.InnerException;

            return BadRequest(new { error = innerException?.Message ?? ex.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest loginRequest)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == loginRequest.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = GenerateJwtToken(user);

        return Ok(new { token });
    }

    // 🔑 JWT Generator
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserId", user.UserID.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}