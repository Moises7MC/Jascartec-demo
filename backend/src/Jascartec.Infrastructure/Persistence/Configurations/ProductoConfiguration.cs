using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jascartec.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> b)
    {
        b.ToTable("productos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.MarcaId).HasColumnName("marca_id").IsRequired();
        b.Property(x => x.Modelo).HasColumnName("modelo").IsRequired();
        b.Property(x => x.Almacenamiento).HasColumnName("almacenamiento").HasMaxLength(20);
        b.Property(x => x.Ram).HasColumnName("ram").HasMaxLength(20);
        b.Property(x => x.Color).HasColumnName("color");
        b.Property(x => x.Precio).HasColumnName("precio").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.CostoReferencial).HasColumnName("costo_referencial").HasColumnType("numeric(10,2)");
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(40);
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Gama).HasColumnName("gama").HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.ImagenUrl).HasColumnName("imagen_url");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();

        b.HasOne(x => x.Marca).WithMany(m => m.Productos).HasForeignKey(x => x.MarcaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Proveedor).WithMany(p => p.Productos).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.SetNull);
    }
}
