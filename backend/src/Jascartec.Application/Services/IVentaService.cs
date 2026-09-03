using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IVentaService
{
    Task<IReadOnlyList<VentaDto>> ListarAsync(CancellationToken ct = default);
    Task<VentaDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<VentaDto> CrearAsync(CrearVentaRequest request, CancellationToken ct = default);
    Task<VentaDto> RegistrarAbonoAsync(int ventaId, RegistrarAbonoRequest request, CancellationToken ct = default);
}
