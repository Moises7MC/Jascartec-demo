using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("clientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        b.Property(x => x.Documento).HasColumnName("documento").HasMaxLength(15).IsRequired();
        b.HasIndex(x => x.Documento).IsUnique();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(15).IsRequired();
        b.Property(x => x.Contacto).HasColumnName("contacto");
        b.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20);
        b.Property(x => x.Email).HasColumnName("email");
        b.Property(x => x.Direccion).HasColumnName("direccion");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
    }
}
