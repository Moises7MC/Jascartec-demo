using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface INegocioService
{
    Task<NegocioDto> ObtenerAsync(CancellationToken ct = default);
    Task<NegocioDto> ActualizarAsync(ActualizarNegocioRequest request, CancellationToken ct = default);
}
