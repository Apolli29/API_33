using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API_3.Data;
using API_3.Models;
using BCrypt.Net;

namespace API_3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _jwtKey = config["Jwt:Key"] ?? "SuperSecretKeyForPractice3ThatIsLongEnough123!";
        _jwtIssuer = config["Jwt:Issuer"] ?? "API_3";
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Login == request.Login)) return BadRequest("Пользователь существует");
        var user = new User { 
            Login = request.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Address = request.Address, 
            Role = UserRole.RegularUser
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok("Регистрация успешна");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Неверный логин или пароль");
        return Ok(new { token = GenerateJwtToken(user) });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, user.Role.ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: _jwtIssuer, claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
public class RegisterRequest { 
    public string Login { get; set; } = ""; 
    public string Password { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Address { get; set; }
}
public class LoginRequest { 
    public string Login { get; set; } = ""; 
    public string Password { get; set; } = "";
}
