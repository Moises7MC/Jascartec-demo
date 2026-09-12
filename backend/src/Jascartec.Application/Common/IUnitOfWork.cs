using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Common;

/// <summary>
/// Agrupa todos los repositorios y expone SaveChangesAsync, para que las operaciones
/// que tocan varias tablas (ej. registrar una venta) se confirmen todas juntas o ninguna.
/// </summary>
public interface IUnitOfWork
{
    INegocioRepository Negocios { get; }
    IUsuarioRepository Usuarios { get; }
    IMarcaRepository Marcas { get; }
    ICategoriaRepository Categorias { get; }
    IProveedorRepository Proveedores { get; }
    IClienteRepository Clientes { get; }
    IProductoRepository Productos { get; }
    IIngresoRepository Ingresos { get; }
    IRepository<IngresoItem> IngresoItems { get; }
    IEquipoRepository Equipos { get; }
    IFacturaRepository Facturas { get; }
    IVentaRepository Ventas { get; }
    ICajaSesionRepository CajaSesiones { get; }
    IRepository<MovimientoCajaManual> MovimientosCaja { get; }
    IRepository<Sucursal> Sucursales { get; }
    IRepository<ProductoStock> ProductoStocks { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
