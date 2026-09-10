using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
{
    public void Configure(EntityTypeBuilder<Equipo> b)
    {
        b.ToTable("equipos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Imei).HasColumnName("imei").HasMaxLength(15).IsRequired();
        b.HasIndex(x => x.Imei).IsUnique();
        b.Property(x => x.Imei2).HasColumnName("imei2").HasMaxLength(15);
        b.HasIndex(x => x.Imei2).IsUnique();
        b.Property(x => x.EstadoFisico).HasColumnName("estado_fisico").HasMaxLength(20).IsRequired();
        b.Property(x => x.CostoCompra).HasColumnName("costo_compra").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso").IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        b.Property(x => x.IngresoId).HasColumnName("ingreso_id");
        b.Property(x => x.EstadoVenta).HasColumnName("estado_venta").HasConversion<string>().HasMaxLength(15).IsRequired();

        b.HasOne(x => x.Producto).WithMany(p => p.Equipos).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Proveedor).WithMany().HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Ingreso).WithMany(i => i.Equipos).HasForeignKey(x => x.IngresoId).OnDelete(DeleteBehavior.SetNull);
    }
}
