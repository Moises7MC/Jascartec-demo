using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> b)
    {
        b.ToTable("proveedores");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        b.Property(x => x.Contacto).HasColumnName("contacto");
        b.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20);
        b.Property(x => x.Email).HasColumnName("email");
        b.Property(x => x.Direccion).HasColumnName("direccion");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
    }
}
