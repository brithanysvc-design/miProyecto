using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillAlexa.BC.Entities;

namespace SkillAlexa.DA.Configurations;

public class ListaCompraConfiguration: IEntityTypeConfiguration<ListaCompra>
{
    public void Configure(EntityTypeBuilder<ListaCompra> builder)
    {
        builder.ToTable("ListasCompra");

        builder.HasKey(l => l.IdLista);

        builder.Property(l => l.IdLista)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(l => l.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.FechaObjetivo)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(l => l.Estado)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.FechaCreacion)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(l => l.FechaModificacion);

        // Relación con ItemLista
        builder.HasMany(l => l.Productos)
            .WithOne(i => i.Lista)
            .HasForeignKey(i => i.IdLista)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(l => new { l.Nombre, l.FechaObjetivo, l.Estado })
            .HasDatabaseName("IX_ListasCompra_Nombre_Fecha_Estado");

        builder.HasIndex(l => l.FechaObjetivo)
            .HasDatabaseName("IX_ListasCompra_FechaObjetivo");
    }
}