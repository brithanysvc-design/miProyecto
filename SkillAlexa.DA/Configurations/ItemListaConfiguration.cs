using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillAlexa.BC.Entities;

namespace SkillAlexa.DA.Configurations;

public class ItemListaConfiguration: IEntityTypeConfiguration<ItemLista>
{
    public void Configure(EntityTypeBuilder<ItemLista> builder)
    {
        builder.ToTable("ItemsLista");

        builder.HasKey(i => i.IdItem);

        builder.Property(i => i.IdItem)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(i => i.IdLista)
            .IsRequired();

        builder.Property(i => i.NombreProducto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Cantidad)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(i => i.Unidad)
            .HasMaxLength(50);

        builder.Property(i => i.Estado)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.FechaCreacion)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(i => i.FechaModificacion);

        // Índices
        builder.HasIndex(i => i.IdLista)
            .HasDatabaseName("IX_ItemsLista_IdLista");

        builder.HasIndex(i => new { i.IdLista, i.Estado })
            .HasDatabaseName("IX_ItemsLista_IdLista_Estado");
    }
    
}