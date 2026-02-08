using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MeuProjetoIA.Auth;

namespace MeuProjetoIA.Features.Auth;

public static class AuthEndpoints
{
    
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        

        group.MapPost("/login", async (LoginRequest request, IConfiguration config) =>
        {

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

           // var jwtSettings = config.GetSection("JwtSetttings").Get<JwtSettings>()
            //                ?? throw new InvalidOperationException("JwtSettings não está configurado corretamente.");

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
    }
}