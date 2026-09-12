using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class MovimientoCajaManualConfiguration : IEntityTypeConfiguration<MovimientoCajaManual>
{
    public void Configure(EntityTypeBuilder<MovimientoCajaManual> b)
    {
        b.ToTable("caja_movimientos_manuales");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.CajaSesionId).HasColumnName("caja_sesion_id").IsRequired();
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.Concepto).HasColumnName("concepto").HasMaxLength(200).IsRequired();
        b.Property(x => x.Monto).HasColumnName("monto").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();

        b.HasOne(x => x.CajaSesion).WithMany(c => c.Movimientos).HasForeignKey(x => x.CajaSesionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
