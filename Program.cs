using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MeuProjetoIA.Features.HelloWorld;
using MeuProjetoIA.Data;
using MeuProjetoIA.Features.Mensagens;
using MeuProjetoIA.Services;

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
}

app.UseHttpsRedirection();

// Mapear features/endpoints 
app.MapGroup("/api")
   .MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// Carregar endpoints de features 
app.MapHelloWorldEndpoints();  
app.MapMensagensEndpoints();

app.Run();