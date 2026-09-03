using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> b)
    {
        b.ToTable("facturas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(30).IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.MontoTotal).HasColumnName("monto_total").HasColumnType("numeric(10,2)").IsRequired();

        b.HasOne(x => x.Proveedor).WithMany(p => p.Facturas).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Restrict);
    }
}
