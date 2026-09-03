using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class NegocioRepository(JascartecDbContext context) : INegocioRepository
{
    public Task<Negocio?> GetAsync(CancellationToken ct = default) =>
        context.Negocios.FirstOrDefaultAsync(ct);

    public void Update(Negocio negocio) => context.Negocios.Update(negocio);
}
