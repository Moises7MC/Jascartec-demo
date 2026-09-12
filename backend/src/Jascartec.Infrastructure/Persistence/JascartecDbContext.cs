using Jascartec.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Persistence;

public class JascartecDbContext(DbContextOptions<JascartecDbContext> options) : DbContext(options)
{
    public DbSet<Negocio> Negocios => Set<Negocio>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<IngresoItem> IngresoItems => Set<IngresoItem>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<Letra> Letras => Set<Letra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaItem> VentaItems => Set<VentaItem>();
    public DbSet<Abono> Abonos => Set<Abono>();
    public DbSet<CajaSesion> CajaSesiones => Set<CajaSesion>();
    public DbSet<MovimientoCajaManual> MovimientosCajaManuales => Set<MovimientoCajaManual>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<ProductoStock> ProductoStocks => Set<ProductoStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cada entidad tiene su propia clase de configuración (IEntityTypeConfiguration<T>)
        // en Persistence/Configurations — así el mapeo no ensucia las entidades del Domain.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JascartecDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
