using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class VentaRepository(JascartecDbContext context) : Repository<Venta>(context), IVentaRepository
{
    private const string Prefijo = "B001-";

    public Task<bool> ExisteNumBoletaAsync(string numBoleta, CancellationToken ct = default) =>
        Set.AnyAsync(v => v.NumBoleta == numBoleta, ct);

    public async Task<string> GenerarSiguienteNumBoletaAsync(CancellationToken ct = default)
    {
        var numeros = await Set
            .Where(v => v.NumBoleta.StartsWith(Prefijo))
            .Select(v => v.NumBoleta)
            .ToListAsync(ct);

        var siguiente = numeros
            .Select(n => int.TryParse(n[Prefijo.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{Prefijo}{siguiente:D5}";
    }

    public async Task<IReadOnlyList<Venta>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await IncluirDetalles(Set).OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id).ToListAsync(ct);

    public Task<Venta?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        IncluirDetalles(Set).FirstOrDefaultAsync(v => v.Id == id, ct);

    private static IQueryable<Venta> IncluirDetalles(IQueryable<Venta> query) => query
        .Include(v => v.Cliente)
        .Include(v => v.Items).ThenInclude(i => i.Equipo).ThenInclude(e => e!.Producto).ThenInclude(p => p.Marca)
        .Include(v => v.Items).ThenInclude(i => i.Producto).ThenInclude(p => p!.Marca)
        .Include(v => v.Abonos);
}
