using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MeuProjetoIA.Features.HelloWorld;
using MeuProjetoIA.Data;
using MeuProjetoIA.Features.Mensagens;
using MeuProjetoIA.Services;
using MeuProjetoIA.Middleware;
using MeuProjetoIA.Auth;
using MeuProjetoIA.Request.Auth;


var builder = WebApplication.CreateBuilder(args);

//Serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();  // necessário para IHttpClientFactory
builder.Services.AddScoped<GroqService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API IA Aprendizado", Version = "v1" });
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                    ?? throw new InvalidOperationException("JwtSettings não está configurado corretamente.");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
};
});

builder.Services.AddSwaggerGen(c =>
{
    // Configuração de segurança JWT no swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. \r\n\r\n "+
        "Entre 'Bearer' [espaço] e então seu token gerado no /api/auth/login.\r\n\r\n"+
        "Exemplo: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});



builder.Services.AddAuthorization();  // Adiciona serviços de autorização (mesmo que não tenhamos políticas específicas ainda)

// Adicionar SQLite + EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Data Source=app.db"));

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API IA v1"));
    //app.UseMiddleware<ApiKeyMiddleware>(); // Middleware de API Key só em desenvolvimento para facilitar testes
}
//app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

// Endpoints básicos soltos (sem grupo desnecessário para debug)
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/debug-env", () => new 
{ 
    Environment = app.Environment.EnvironmentName, 
    Timestamp = DateTime.UtcNow 
});

// Carregar endpoints de features 
app.MapHelloWorldEndpoints();  
app.MapMensagensEndpoints();
app.MapAuthEndpoints();

app.Run();