using Microsoft.EntityFrameworkCore;
using BoloGestaoAPI.Models;

namespace BoloGestaoAPI.Data;

public class BoloDbContext : DbContext
{
    public BoloDbContext(DbContextOptions<BoloDbContext> options) : base(options) { }

    public DbSet<Venda> Vendas { get; set; }
    public DbSet<MaterialUsado> MateriaisUsados { get; set; }
    public DbSet<Gasto> Gastos { get; set; }
}