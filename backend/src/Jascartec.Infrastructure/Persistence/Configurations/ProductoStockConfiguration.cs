using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class ProductoStockConfiguration : IEntityTypeConfiguration<ProductoStock>
{
    public void Configure(EntityTypeBuilder<ProductoStock> b)
    {
        b.ToTable("producto_stock");
        b.HasKey(x => new { x.ProductoId, x.SucursalId });
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.SucursalId).HasColumnName("sucursal_id");
        b.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired().HasDefaultValue(0);

        b.HasOne(x => x.Producto).WithMany(p => p.Stocks).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
    }
}
