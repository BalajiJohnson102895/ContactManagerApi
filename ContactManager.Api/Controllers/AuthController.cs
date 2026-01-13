using ContactManager.Api.Data;
using ContactManager.Api.DTO;
using ContactManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null || user.PasswordHash != request.Password)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token, user.Email, user.FullName });
        //if (request.Username != "admin" || request.Password != "admin123")
        //    return Unauthorized("Invalid credentials");

        //var claims = new[]
        //{
        //    new Claim(ClaimTypes.Name, request.Username)
        //};

        //var key = new SymmetricSecurityKey(
        //    Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        //);

        //var token = new JwtSecurityToken(
        //    claims: claims,
        //    expires: DateTime.UtcNow.AddHours(1),
        //    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        //);

        //return Ok(new
        //{
        //    token = new JwtSecurityTokenHandler().WriteToken(token)
        //});
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("FullName", user.FullName)
    };

        var token = new JwtSecurityToken(
            issuer: "yourdomain.com",
            audience: "yourdomain.com",
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}



