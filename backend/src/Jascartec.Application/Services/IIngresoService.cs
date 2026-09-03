using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IIngresoService
{
    Task<IReadOnlyList<IngresoDto>> ListarAsync(CancellationToken ct = default);
    Task<IngresoDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IngresoDto> CrearAsync(CrearIngresoRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
