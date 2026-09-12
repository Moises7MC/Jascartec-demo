using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> b)
    {
        b.ToTable("ventas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.NumBoleta).HasColumnName("num_boleta").HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.NumBoleta).IsUnique();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        // La BD usa "Crédito" (con tilde); el enum de C# no puede llevar tilde en el nombre,
        // así que se traduce explícitamente en ambas direcciones.
        b.Property(x => x.FormaPago).HasColumnName("forma_pago").HasMaxLength(10).IsRequired()
            .HasConversion(
                v => v == FormaPago.Credito ? "Crédito" : "Contado",
                v => v == "Crédito" ? FormaPago.Credito : FormaPago.Contado);
        b.Property(x => x.MedioPago).HasColumnName("medio_pago").HasConversion<string>().HasMaxLength(15);
        b.Property(x => x.FechaPagoAcordada).HasColumnName("fecha_pago_acordada");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(10).IsRequired()
            .HasDefaultValue(EstadoBoleta.Activa);
        b.Property(x => x.FechaAnulacion).HasColumnName("fecha_anulacion");

        b.Property(x => x.MontoInicial).HasColumnName("monto_inicial").HasColumnType("numeric(10,2)");
        b.Property(x => x.Recargo).HasColumnName("recargo").HasColumnType("numeric(10,2)").HasDefaultValue(0m);
        b.Property(x => x.FrecuenciaPago).HasColumnName("frecuencia_pago").HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.NumCuotas).HasColumnName("num_cuotas");

        b.HasOne(x => x.Cliente).WithMany(c => c.Ventas).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.SetNull);
        b.Ignore(x => x.Total);
        b.Ignore(x => x.MontoPagado);
        b.Ignore(x => x.SaldoPendiente);
    }
}
