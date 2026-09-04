using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class IngresoItemConfiguration : IEntityTypeConfiguration<IngresoItem>
{
    public void Configure(EntityTypeBuilder<IngresoItem> b)
    {
        b.ToTable("ingreso_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.IngresoId).HasColumnName("ingreso_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired();
        b.Property(x => x.CostoUnit).HasColumnName("costo_unit").HasColumnType("numeric(10,2)").IsRequired();

        b.HasOne(x => x.Ingreso).WithMany(i => i.Items).HasForeignKey(x => x.IngresoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
    }
}
