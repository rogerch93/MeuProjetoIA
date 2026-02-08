namespace MeuProjetoIA.Auth;

public class JwtSettings
{
    public string SecuretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MeuProjetoIA";
    public string Audience { get; set; } = "MeuProjetoIA";
    public int ExpiresInMinutes { get; set; } = 60; //1 hora
}