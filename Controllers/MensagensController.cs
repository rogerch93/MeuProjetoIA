using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeuProjetoIA.Data;
using MeuProjetoIA.Models;
using MeuProjetoIA.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace MeuProjetoIA.Controllers;

[ApiController]
[Route("api/mensagens")]
[Authorize]
public class MensagensController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly GroqService _groq;

    public MensagensController(AppDbContext db, GroqService groq)
    {
        _db = db;
        _groq = groq;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var mensagens = await _db.Mensagens
                .OrderByDescending(m => m.CriadoEm)
                .ToListAsync();
        
        return Ok(mensagens);
    }

    [HttpPost("ia")]
    [EnableRateLimiting("fixed")] // esta aplicado o limite chamado fixed
    [Authorize]
    public async Task<IActionResult>GenerateIA([FromBody] MensagemIA request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        return BadRequest("Prompt é obrigatório.");

        string respostaIA;
        try
        {
            respostaIA = await _groq.GenerateResponseAsync(request.Prompt);
        }
        catch (Exception ex)
        {
            return Problem($"Erro na IA: {ex.Message}");
        }

        var mensagem =new MensagemIA
        {
            Prompt = request.Prompt,
            Resposta = respostaIA,
            CriadoEm = DateTime.UtcNow,
            Usuario = User.Identity?.Name ?? Environment.UserName
        };

        _db.Mensagens.Add(mensagem);
        await _db.SaveChangesAsync();

        return Created($"/api/mensagens/{mensagem.Id}", mensagem);
    }
}