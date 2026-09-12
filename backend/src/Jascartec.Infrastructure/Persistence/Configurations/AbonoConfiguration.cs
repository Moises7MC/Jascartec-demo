using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class AbonoConfiguration : IEntityTypeConfiguration<Abono>
{
    public void Configure(EntityTypeBuilder<Abono> b)
    {
        b.ToTable("abonos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.VentaId).HasColumnName("venta_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Monto).HasColumnName("monto").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.MedioPago).HasColumnName("medio_pago").HasConversion<string>().HasMaxLength(15).IsRequired()
            .HasDefaultValue(Jascartec.Domain.Enums.MedioPago.Efectivo);

        b.HasOne(x => x.Venta).WithMany(v => v.Abonos).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Cascade);
    }
}
