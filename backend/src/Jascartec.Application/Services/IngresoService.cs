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
            throw new BusinessRuleException("El ingreso debe traer al menos un equipo.");

        if (await unitOfWork.Proveedores.GetByIdAsync(request.ProveedorId, ct) is null)
            throw new BusinessRuleException($"El proveedor con id '{request.ProveedorId}' no existe.");

        // Un IMEI duplicado dentro del mismo request, o ya existente en la BD, se rechaza.
        var imeisEnRequest = new HashSet<string>();
        foreach (var item in request.Items)
        {
            if (!imeisEnRequest.Add(item.Imei))
                throw new BusinessRuleException($"El IMEI '{item.Imei}' está repetido en el ingreso.");
            if (await unitOfWork.Equipos.ExisteImeiAsync(item.Imei, ct))
                throw new BusinessRuleException($"El IMEI '{item.Imei}' ya está registrado en el sistema.");
        }

        var ingreso = new Ingreso
        {
            Fecha = request.Fecha,
            ProveedorId = request.ProveedorId,
            NumeroFactura = request.NumeroFactura
        };
        await unitOfWork.Ingresos.AddAsync(ingreso, ct);
        await unitOfWork.SaveChangesAsync(ct); // necesitamos el Id generado antes de crear los equipos

        foreach (var item in request.Items)
        {
            if (await unitOfWork.Productos.GetByIdAsync(item.ProductoId, ct) is null)
                throw new BusinessRuleException($"El producto con id '{item.ProductoId}' no existe.");

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
        await unitOfWork.SaveChangesAsync(ct);

        return await ObtenerAsync(ingreso.Id, ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var ingreso = await unitOfWork.Ingresos.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Ingreso", id);
        if (ingreso.Equipos.Any(e => e.EstadoVenta != EstadoVenta.Disponible))
            throw new BusinessRuleException("No se puede eliminar: alguno de sus equipos ya fue vendido.");

        // Los equipos de este ingreso representan el inventario que trajo — se eliminan
        // junto con él (misma regla que ya aplicaba el frontend en memoria).
        foreach (var equipo in ingreso.Equipos.ToList())
            unitOfWork.Equipos.Remove(equipo);
        unitOfWork.Ingresos.Remove(ingreso);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static IngresoDto ToDto(Ingreso i) => new(
        i.Id, i.Fecha, i.ProveedorId, i.Proveedor.Nombre, i.NumeroFactura,
        i.Equipos.Select(e => new EquipoDto(e.Id, e.ProductoId, $"{e.Producto.Marca.Nombre} {e.Producto.Modelo}", e.Imei, e.EstadoFisico, e.CostoCompra, e.FechaIngreso, e.EstadoVenta.ToString())).ToList());
}
