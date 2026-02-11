using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MeuProjetoIA.Auth;
using System.Data.Common;
using MeuProjetoIA.Data;
using Microsoft.EntityFrameworkCore;
using MeuProjetoIA.Models;

namespace MeuProjetoIA.Request.Auth;

public static class AuthEndpoints
{
    
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, AppDbContext db, IConfiguration config) =>
        {
            var user = db.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }   
            
            var section = config.GetSection("JwtSettings");
            if (!section.Exists())
            {
                return Results.Problem("Seção JwtSettings não encontrada no configuration");
            }
            var jwtSettings = section.Get<JwtSettings>();
            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
            {
                return Results.Problem("JwtSettings carregado, mas SecretKey vazia ou null");
            }

            // Usuário fixo para demonstração
            if(request.Username != "admin" || request.Password != "12345")
            {
                return Results.Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role,"Admin"),
                new Claim("custom-claim", "valor")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes),
                signingCredentials: creds);
            
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Results.Ok(new LoginResponse { Token = jwt });
        }).WithName("Login");

        group.MapPost("/register", async(RegisterRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Username e password são obrigatórios");
            }

            if (request.Password.Length < 8)
            {
                return Results.BadRequest("Password deve ter pelo menos 8 caracteres");
            }
            
            var existing = await db.Users.AnyAsync(u => u.Username == request.Username);
            if (existing)
            {
                return Results.Conflict("Username já existe");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hash,
                Role = "User",
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/auth/users/{user.Id}", new { user.Id, user.Username });
        }).WithName("Register");
    }
}