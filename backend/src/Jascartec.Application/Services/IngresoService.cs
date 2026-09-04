using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class IngresoService(IUnitOfWork unitOfWork) : IIngresoService
{
    public async Task<IReadOnlyList<IngresoDto>> ListarAsync(CancellationToken ct = default)
    {
        var ingresos = await unitOfWork.Ingresos.GetAllWithDetailsAsync(ct);
        return ingresos.Select(ToDto).ToList();
    }

    public async Task<IngresoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var ingreso = await unitOfWork.Ingresos.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Ingreso", id);
        return ToDto(ingreso);
    }

    public async Task<IngresoDto> CrearAsync(CrearIngresoRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new BusinessRuleException("El ingreso debe traer al menos un producto.");

        if (await unitOfWork.Proveedores.GetByIdAsync(request.ProveedorId, ct) is null)
            throw new BusinessRuleException($"El proveedor con id '{request.ProveedorId}' no existe.");

        // Un IMEI duplicado dentro del mismo request, o ya existente en la BD, se rechaza.
        var imeisEnRequest = new HashSet<string>();
        foreach (var item in request.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Imei))
            {
                if (!imeisEnRequest.Add(item.Imei))
                    throw new BusinessRuleException($"El IMEI '{item.Imei}' está repetido en el ingreso.");
                if (await unitOfWork.Equipos.ExisteImeiAsync(item.Imei, ct))
                    throw new BusinessRuleException($"El IMEI '{item.Imei}' ya está registrado en el sistema.");
            }
        }

        var ingreso = new Ingreso
        {
            Fecha = request.Fecha,
            ProveedorId = request.ProveedorId,
            NumeroFactura = request.NumeroFactura,
            CreadoEn = DateTimeOffset.UtcNow
        };
        await unitOfWork.Ingresos.AddAsync(ingreso, ct);
        await unitOfWork.SaveChangesAsync(ct); // necesitamos el Id generado antes de crear los items

        foreach (var item in request.Items)
        {
            var producto = await unitOfWork.Productos.GetByIdWithDetailsAsync(item.ProductoId, ct)
                ?? throw new BusinessRuleException($"El producto con id '{item.ProductoId}' no existe.");

            if (producto.Categoria.RequiereImei)
            {
                if (string.IsNullOrWhiteSpace(item.Imei))
                    throw new BusinessRuleException($"El producto '{producto.Modelo}' requiere IMEI.");

                await unitOfWork.Equipos.AddAsync(new Equipo
                {
                    ProductoId = item.ProductoId,
                    Imei = item.Imei,
                    CostoCompra = item.CostoUnit,
                    FechaIngreso = request.Fecha,
                    ProveedorId = request.ProveedorId,
                    IngresoId = ingreso.Id
                }, ct);
            }
            else
            {
                if (item.Cantidad is null or < 1)
                    throw new BusinessRuleException($"El producto '{producto.Modelo}' se controla por cantidad: indique cuántas unidades ingresan.");

                await unitOfWork.IngresoItems.AddAsync(new IngresoItem
                {
                    IngresoId = ingreso.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad.Value,
                    CostoUnit = item.CostoUnit
                }, ct);

                producto.StockCantidad += item.Cantidad.Value;
                unitOfWork.Productos.Update(producto);
            }
        }
        await unitOfWork.SaveChangesAsync(ct);

        return await ObtenerAsync(ingreso.Id, ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var ingreso = await unitOfWork.Ingresos.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Ingreso", id);
        if (ingreso.Equipos.Any(e => e.EstadoVenta != EstadoVenta.Disponible))
            throw new BusinessRuleException("No se puede eliminar: alguno de sus equipos ya fue vendido.");

        // Los equipos de este ingreso representan el inventario que trajo — se eliminan
        // junto con él (misma regla que ya aplicaba el frontend en memoria). Las líneas por
        // cantidad le devuelven su stock al producto antes de borrarse.
        foreach (var equipo in ingreso.Equipos.ToList())
            unitOfWork.Equipos.Remove(equipo);
        foreach (var item in ingreso.Items.ToList())
        {
            item.Producto.StockCantidad = Math.Max(0, item.Producto.StockCantidad - item.Cantidad);
            unitOfWork.Productos.Update(item.Producto);
            unitOfWork.IngresoItems.Remove(item);
        }
        unitOfWork.Ingresos.Remove(ingreso);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static IngresoDto ToDto(Ingreso i) => new(
        i.Id, i.Fecha, i.ProveedorId, i.Proveedor.Nombre, i.NumeroFactura,
        i.Equipos.Select(e => new EquipoDto(e.Id, e.ProductoId, $"{e.Producto.Marca.Nombre} {e.Producto.Modelo}", e.Imei, e.EstadoFisico, e.CostoCompra, e.FechaIngreso, e.EstadoVenta.ToString())).ToList(),
        i.Items.Select(it => new IngresoItemDto(it.Id, it.ProductoId, $"{it.Producto.Marca.Nombre} {it.Producto.Modelo}", it.Cantidad, it.CostoUnit)).ToList(),
        i.CreadoEn);
}
