using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface ICajaService
{
    Task<CajaSesionDto?> ObtenerAbiertaAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CajaSesionDto>> ListarAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<CajaSesionDto> AbrirAsync(AbrirCajaRequest request, int usuarioId, CancellationToken ct = default);
    Task<CajaSesionDto> CerrarAsync(int id, CerrarCajaRequest request, int usuarioId, CancellationToken ct = default);
    Task<CajaSesionDto> RegistrarMovimientoAsync(int cajaSesionId, CrearMovimientoCajaRequest request, int usuarioId, CancellationToken ct = default);
}
