using Jascartec.Application.Common;
using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public class EquipoService(IUnitOfWork unitOfWork) : IEquipoService
{
    public async Task<IReadOnlyList<EquipoDto>> ListarDisponiblesAsync(int productoId, CancellationToken ct = default)
    {
        var producto = await unitOfWork.Productos.GetByIdWithDetailsAsync(productoId, ct)
            ?? throw new NotFoundException("Producto", productoId);

        var equipos = await unitOfWork.Equipos.GetDisponiblesPorProductoAsync(productoId, ct);
        var nombreProducto = $"{producto.Marca.Nombre} {producto.Modelo}";

        return equipos
            .OrderBy(e => e.Id)
            .Select(e => new EquipoDto(e.Id, e.ProductoId, nombreProducto, e.Imei, e.Imei2, e.EstadoFisico, e.CostoCompra, e.FechaIngreso, e.EstadoVenta.ToString()))
            .ToList();
    }
}
