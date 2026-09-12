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
        if (await unitOfWork.Sucursales.GetByIdAsync(request.SucursalId, ct) is null)
            throw new BusinessRuleException($"La sucursal con id '{request.SucursalId}' no existe.");

        // Un IMEI duplicado dentro del mismo request, o ya existente en la BD, se rechaza —
        // sin importar si aparece como el principal o como el segundo (dual SIM) de otra línea.
        var imeisEnRequest = new HashSet<string>();
        foreach (var item in request.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Imei) && !string.IsNullOrWhiteSpace(item.Imei2) && item.Imei == item.Imei2)
                throw new BusinessRuleException("El IMEI 2 no puede ser igual al IMEI principal.");

            foreach (var imei in new[] { item.Imei, item.Imei2 })
            {
                if (string.IsNullOrWhiteSpace(imei)) continue;
                if (!imeisEnRequest.Add(imei))
                    throw new BusinessRuleException($"El IMEI '{imei}' está repetido en el ingreso.");
                if (await unitOfWork.Equipos.ExisteImeiAsync(imei, ct))
                    throw new BusinessRuleException($"El IMEI '{imei}' ya está registrado en el sistema.");
            }
        }

        var ingreso = new Ingreso
        {
            Fecha = request.Fecha,
            ProveedorId = request.ProveedorId,
            NumeroFactura = request.NumeroFactura,
            SucursalId = request.SucursalId,
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
                    Imei2 = item.Imei2,
                    CostoCompra = item.CostoUnit,
                    FechaIngreso = request.Fecha,
                    ProveedorId = request.ProveedorId,
                    IngresoId = ingreso.Id,
                    SucursalId = request.SucursalId
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

                await SumarStockAsync(item.ProductoId, request.SucursalId, item.Cantidad.Value, ct);
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
            await SumarStockAsync(item.ProductoId, ingreso.SucursalId, -item.Cantidad, ct);
            unitOfWork.IngresoItems.Remove(item);
        }
        unitOfWork.Ingresos.Remove(ingreso);
        await unitOfWork.SaveChangesAsync(ct);
    }

    // Suma (o resta, con delta negativo) al contador de stock de un producto por cantidad en una
    // sucursal puntual — crea la fila de ProductoStock si todavía no existía. Nunca deja quedar
    // un número negativo (por si se elimina un ingreso viejo y el stock ya se movió de más).
    private async Task SumarStockAsync(int productoId, int sucursalId, int delta, CancellationToken ct)
    {
        var stocks = await unitOfWork.ProductoStocks.GetAllAsync(ct);
        var fila = stocks.FirstOrDefault(s => s.ProductoId == productoId && s.SucursalId == sucursalId);
        if (fila is null)
        {
            await unitOfWork.ProductoStocks.AddAsync(new ProductoStock { ProductoId = productoId, SucursalId = sucursalId, Cantidad = Math.Max(0, delta) }, ct);
        }
        else
        {
            fila.Cantidad = Math.Max(0, fila.Cantidad + delta);
            unitOfWork.ProductoStocks.Update(fila);
        }
    }

    private static IngresoDto ToDto(Ingreso i) => new(
        i.Id, i.Fecha, i.ProveedorId, i.Proveedor.Nombre, i.NumeroFactura, i.SucursalId, i.Sucursal.Nombre,
        i.Equipos.Select(e => new EquipoDto(e.Id, e.ProductoId, $"{e.Producto.Marca.Nombre} {e.Producto.Modelo}", e.Imei, e.Imei2, e.EstadoFisico, e.CostoCompra, e.FechaIngreso, e.EstadoVenta.ToString(), i.SucursalId, i.Sucursal.Nombre)).ToList(),
        i.Items.Select(it => new IngresoItemDto(it.Id, it.ProductoId, $"{it.Producto.Marca.Nombre} {it.Producto.Modelo}", it.Cantidad, it.CostoUnit)).ToList(),
        i.CreadoEn);
}
