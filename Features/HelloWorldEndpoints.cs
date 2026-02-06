using Microsoft.AspNetCore.Builder;

namespace MeuProjetoIA.Features.HelloWorld;

public static class HelloWorldEndpoints
{
    public static void MapHelloWorldEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/hello");

        group.MapGet("/", () => new 
        { 
            mensagem = "API IA rodando com estrutura decente", 
            hora = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            ambiente = app.Environment.EnvironmentName 
        });
    }
}