using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> b)
    {
        b.ToTable("sucursales");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired().HasDefaultValue(true);

        // 3 sucursales de arranque — el negocio empieza con estas; se pueden renombrar o
        // desactivar después desde el propio sistema (no hace falta agregar más ahora).
        b.HasData(
            new Sucursal { Id = 1, Nombre = "Sucursal 1", Activa = true },
            new Sucursal { Id = 2, Nombre = "Sucursal 2", Activa = true },
            new Sucursal { Id = 3, Nombre = "Sucursal 3", Activa = true }
        );
    }
}
