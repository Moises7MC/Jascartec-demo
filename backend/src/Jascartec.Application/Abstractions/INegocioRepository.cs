using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface INegocioRepository
{
    Task<Negocio?> GetAsync(CancellationToken ct = default);
    void Update(Negocio negocio);
}
