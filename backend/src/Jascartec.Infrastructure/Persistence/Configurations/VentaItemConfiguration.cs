using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class VentaItemConfiguration : IEntityTypeConfiguration<VentaItem>
{
    public void Configure(EntityTypeBuilder<VentaItem> b)
    {
        b.ToTable("venta_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.VentaId).HasColumnName("venta_id").IsRequired();
        b.Property(x => x.EquipoId).HasColumnName("equipo_id").IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired().HasDefaultValue(true);
        // Un equipo solo puede estar en UNA venta activa a la vez; si esa venta se anula,
        // "activo" pasa a false y el equipo_id queda libre para una venta futura.
        b.HasIndex(x => x.EquipoId).IsUnique().HasFilter("activo = true");
        b.Property(x => x.PrecioUnit).HasColumnName("precio_unit").HasColumnType("numeric(10,2)").IsRequired();

        b.HasOne(x => x.Venta).WithMany(v => v.Items).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Equipo).WithMany(e => e.VentaItems).HasForeignKey(x => x.EquipoId).OnDelete(DeleteBehavior.Restrict);
    }
}
