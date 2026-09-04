using Jascartec.Application.Abstractions;
using Jascartec.Application.Common;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;

namespace Jascartec.Infrastructure.Repositories;

public class UnitOfWork(
    JascartecDbContext context,
    INegocioRepository negocios,
    IUsuarioRepository usuarios,
    IMarcaRepository marcas,
    ICategoriaRepository categorias,
    IProveedorRepository proveedores,
    IClienteRepository clientes,
    IProductoRepository productos,
    IIngresoRepository ingresos,
    IRepository<IngresoItem> ingresoItems,
    IEquipoRepository equipos,
    IFacturaRepository facturas,
    IVentaRepository ventas) : IUnitOfWork
{
    public INegocioRepository Negocios => negocios;
    public IUsuarioRepository Usuarios => usuarios;
    public IMarcaRepository Marcas => marcas;
    public ICategoriaRepository Categorias => categorias;
    public IProveedorRepository Proveedores => proveedores;
    public IClienteRepository Clientes => clientes;
    public IProductoRepository Productos => productos;
    public IIngresoRepository Ingresos => ingresos;
    public IRepository<IngresoItem> IngresoItems => ingresoItems;
    public IEquipoRepository Equipos => equipos;
    public IFacturaRepository Facturas => facturas;
    public IVentaRepository Ventas => ventas;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
