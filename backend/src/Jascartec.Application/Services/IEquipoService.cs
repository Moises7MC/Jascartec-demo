using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IEquipoService
{
    /// <summary>Equipos (IMEIs) Disponibles de un producto — para el selector del carrito de venta.</summary>
    Task<IReadOnlyList<EquipoDto>> ListarDisponiblesAsync(int productoId, CancellationToken ct = default);
}
