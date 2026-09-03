using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IMarcaService
{
    Task<IReadOnlyList<MarcaDto>> ListarAsync(CancellationToken ct = default);
    Task<MarcaDto> CrearAsync(GuardarMarcaRequest request, CancellationToken ct = default);
    Task<MarcaDto> ActualizarAsync(int id, GuardarMarcaRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
