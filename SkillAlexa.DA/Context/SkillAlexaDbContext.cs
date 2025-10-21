using Microsoft.EntityFrameworkCore;
using SkillAlexa.BC.Entities;

namespace SkillAlexa.DA.Context;

public class SkillAlexaDbContext:DbContext
{
    public SkillAlexaDbContext(DbContextOptions<SkillAlexaDbContext> options) : base(options)
    {
        
    }
    public DbSet<ListaCompra> ListasCompra { get; set; }
    public DbSet<ItemLista> ItemsLista { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkillAlexaDbContext).Assembly);
    }
}