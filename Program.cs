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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

//Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

//Serviços
builder.Services.AddHttpClient();
builder.Services.AddScoped<GroqService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MeuProjetoIA",
        Version= "v1"
    });

    //Configuração para mostrar o cadeado baerer bi Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer.\r\n\r\n" +
                      "Digite 'Bearer' [espaço] seguido do seu token gerado no /api/auth/login.\r\n\r\n" +
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
           new string[] {} // esta sendo aplicado para todos os endpoints
        }
    });

});

// Adicionar SQLite + EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Data Source=app.db"));

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

builder.Services.AddAuthorization();  // Adiciona serviços de autorização (mesmo que não tenhamos políticas específicas ainda)

//Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 10; // limite de 10 requisições
        opt.Window = TimeSpan.FromMinutes(1); //por minuto
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5; //fila de espera
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.Append("Retry-After", "60");
        await context.HttpContext.Response.WriteAsync("Muitas requisições. Tente novamente em 1 minuto.");
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();