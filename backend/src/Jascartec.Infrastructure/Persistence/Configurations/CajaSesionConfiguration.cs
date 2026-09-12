using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class CajaSesionConfiguration : IEntityTypeConfiguration<CajaSesion>
{
    public void Configure(EntityTypeBuilder<CajaSesion> b)
    {
        b.ToTable("caja_sesiones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(10).IsRequired();

        b.Property(x => x.UsuarioAperturaId).HasColumnName("usuario_apertura_id").IsRequired();
        b.Property(x => x.AbiertaEn).HasColumnName("abierta_en").IsRequired();
        b.Property(x => x.MontoInicial).HasColumnName("monto_inicial").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.ObservacionesApertura).HasColumnName("observaciones_apertura").HasMaxLength(500);

        b.Property(x => x.UsuarioCierreId).HasColumnName("usuario_cierre_id");
        b.Property(x => x.CerradaEn).HasColumnName("cerrada_en");
        b.Property(x => x.MontoContadoCierre).HasColumnName("monto_contado_cierre").HasColumnType("numeric(10,2)");
        b.Property(x => x.ObservacionesCierre).HasColumnName("observaciones_cierre").HasMaxLength(500);

        // Solo puede haber una caja abierta a la vez en todo el negocio (índice único parcial).
        b.HasIndex(x => x.Estado).IsUnique().HasFilter("estado = 'Abierta'").HasDatabaseName("IX_caja_sesiones_una_abierta");

        b.HasOne(x => x.UsuarioApertura).WithMany().HasForeignKey(x => x.UsuarioAperturaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioCierre).WithMany().HasForeignKey(x => x.UsuarioCierreId).OnDelete(DeleteBehavior.Restrict);
    }
}
