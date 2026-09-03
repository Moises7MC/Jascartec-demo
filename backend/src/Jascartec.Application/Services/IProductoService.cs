using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IProductoService
{
    Task<IReadOnlyList<ProductoDto>> ListarAsync(CancellationToken ct = default);
    Task<ProductoDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<ProductoDto> CrearAsync(GuardarProductoRequest request, CancellationToken ct = default);
    Task<ProductoDto> ActualizarAsync(int id, GuardarProductoRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
