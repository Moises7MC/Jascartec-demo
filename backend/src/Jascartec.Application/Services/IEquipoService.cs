using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IEquipoService
{
    /// <summary>Equipos (IMEIs) Disponibles de un producto — para el selector del carrito de venta.
    /// sucursalId filtra a los que están físicamente en esa sucursal; null trae de todas.</summary>
    Task<IReadOnlyList<EquipoDto>> ListarDisponiblesAsync(int productoId, int? sucursalId = null, CancellationToken ct = default);
}
