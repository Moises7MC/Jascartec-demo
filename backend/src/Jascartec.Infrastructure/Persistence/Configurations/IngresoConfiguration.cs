using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class IngresoConfiguration : IEntityTypeConfiguration<Ingreso>
{
    public void Configure(EntityTypeBuilder<Ingreso> b)
    {
        b.ToTable("ingresos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        b.Property(x => x.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(30);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();

        b.HasOne(x => x.Proveedor).WithMany(p => p.Ingresos).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
    }
}
