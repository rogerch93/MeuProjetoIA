using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MeuProjetoIA.Data;
using MeuProjetoIA.Models;
using MeuProjetoIA.Request.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using MeuProjetoIA.Auth;

namespace MeuProjetoIA.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPostAttribute("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username== request.Username);
        if(user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        return Unauthorized("Credenciais Inválidas");

        var jwtSettings = _config.GetSection("JwtSettings");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token= new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"])),
            signingCredentials: creds);

        return Ok(new{Token = new JwtSecurityTokenHandler().WriteToken(token) });
        
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return BadRequest("Username e password são obrigatótios");

        if (req.Password.Length < 8 )
        return BadRequest("Senha deve ter pelo menos 8 caracteres");

        var existing = await _db.Users.AnyAsync(u => u.Username == req.Username);
        if(existing)
        return Conflict("Username já Existe");

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var user = new User
        {
            Username = req.Username,
            PasswordHash = hash,
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created($"/api/auth/users/{user.Id}", new {user.Id, user.Username});
    }


}

