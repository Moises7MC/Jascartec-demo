using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.NombreUsuario).HasColumnName("usuario").HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.NombreUsuario).IsUnique();
        b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        b.Property(x => x.Rol).HasColumnName("rol").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Iniciales).HasColumnName("iniciales").HasMaxLength(4).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
    }
}
