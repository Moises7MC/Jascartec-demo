using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken ct = default);
    Task<CategoriaDto> CrearAsync(GuardarCategoriaRequest request, CancellationToken ct = default);
    Task<CategoriaDto> ActualizarAsync(int id, GuardarCategoriaRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
