using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class NegocioConfiguration : IEntityTypeConfiguration<Negocio>
{
    public void Configure(EntityTypeBuilder<Negocio> b)
    {
        b.ToTable("negocio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.RazonSocial).HasColumnName("razon_social").IsRequired();
        b.Property(x => x.Ruc).HasColumnName("ruc").HasMaxLength(11).IsRequired();
        b.Property(x => x.Direccion).HasColumnName("direccion").IsRequired();
        b.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20).IsRequired();
        b.Property(x => x.Email).HasColumnName("email").IsRequired();
        b.Property(x => x.Web).HasColumnName("web");

        b.HasData(new Negocio
        {
            Id = 1,
            RazonSocial = "Jascartec S.A.C.",
            Ruc = "20601234567",
            Direccion = "Calle Cajamarca 424 - Chepén, Chepén, Peru, 13871, La Libertad",
            Telefono = "+51  920 734 014",
            Email = "jasmany6@hotmail.com",
            Web = "www.jascartec.com"
        });
    }
}
