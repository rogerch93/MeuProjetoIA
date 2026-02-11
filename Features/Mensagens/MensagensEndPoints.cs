using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using MeuProjetoIA.Data;
using MeuProjetoIA.Models;
using MeuProjetoIA.Services;

namespace MeuProjetoIA.Features.Mensagens;

public static class MensagensEndpoints
{
    public static void MapMensagensEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/mensagens")
                    .WithTags("Mensagens");
                    //.RequireAuthorization();

        // GET lista
        group.MapGet("/", async (AppDbContext db) =>
        {
            var mensagens = await db.Mensagens
                .OrderByDescending(m => m.CriadoEm)
                .ToListAsync();
            return Results.Ok(mensagens);
        })
        .WithName("GetMensagens");

        // POST cria simples
        group.MapPost("/", async (MensagemIA novaMensagem, AppDbContext db) =>
        {
            db.Mensagens.Add(novaMensagem);
            await db.SaveChangesAsync();
            return Results.Created($"/api/mensagens/{novaMensagem.Id}", novaMensagem);
        })
        .WithName("CreateMensagem");

        // POST com IA - o que você quer
        group.MapPost("/ia", async (MensagemIA request, AppDbContext db, GroqService groq) =>
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return Results.BadRequest("Prompt é obrigatório.");

            string respostaIA;
            try
            {
                respostaIA = await groq.GenerateResponseAsync(request.Prompt);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro na IA: {ex.Message}");
            }

            var mensagem = new MensagemIA
            {
                Prompt = request.Prompt,
                Resposta = respostaIA,
                CriadoEm = DateTime.UtcNow,
                Usuario = Environment.UserName
            };

            db.Mensagens.Add(mensagem);
            await db.SaveChangesAsync();

            return Results.Created($"/api/mensagens/{mensagem.Id}", mensagem);
        })
        .WithName("GenerateComIA");
        
    }
}