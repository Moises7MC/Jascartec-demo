using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IFacturaService
{
    Task<IReadOnlyList<FacturaDto>> ListarAsync(CancellationToken ct = default);
    Task<FacturaDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<FacturaDto> CrearAsync(CrearFacturaRequest request, CancellationToken ct = default);
    Task<FacturaDto> ToggleLetraPagadaAsync(int facturaId, int numeroLetra, CancellationToken ct = default);
}
