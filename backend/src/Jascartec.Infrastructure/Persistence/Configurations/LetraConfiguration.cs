using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class LetraConfiguration : IEntityTypeConfiguration<Letra>
{
    public void Configure(EntityTypeBuilder<Letra> b)
    {
        b.ToTable("letras");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.FacturaId).HasColumnName("factura_id").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Monto).HasColumnName("monto").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento").IsRequired();
        b.Property(x => x.Pagada).HasColumnName("pagada").IsRequired();
        b.Property(x => x.FechaPago).HasColumnName("fecha_pago");
        b.HasIndex(x => new { x.FacturaId, x.Numero }).IsUnique();

        b.HasOne(x => x.Factura).WithMany(f => f.Letras).HasForeignKey(x => x.FacturaId).OnDelete(DeleteBehavior.Cascade);
    }
}
