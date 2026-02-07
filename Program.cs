using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MeuProjetoIA.Features.HelloWorld;
using MeuProjetoIA.Data;
using MeuProjetoIA.Features.Mensagens;
using MeuProjetoIA.Services;
using MeuProjetoIA.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();  // necessário para IHttpClientFactory
builder.Services.AddScoped<GroqService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API IA Aprendizado", Version = "v1" });
});

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
    app.UseMiddleware<ApiKeyMiddleware>(); // Middleware de API Key só em desenvolvimento para facilitar testes
}

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


app.Run();