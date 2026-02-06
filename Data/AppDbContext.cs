using MeuProjetoIA.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoIA.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MensagemIA> Mensagens {get;set;}
}