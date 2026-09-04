using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Services;

public class FacturaService(IUnitOfWork unitOfWork) : IFacturaService
{
    public async Task<IReadOnlyList<FacturaDto>> ListarAsync(CancellationToken ct = default)
    {
        var facturas = await unitOfWork.Facturas.GetAllWithLetrasAsync(ct);
        return facturas.Select(ToDto).ToList();
    }

    public async Task<FacturaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var factura = await unitOfWork.Facturas.GetByIdWithLetrasAsync(id, ct) ?? throw new NotFoundException("Factura", id);
        return ToDto(factura);
    }

    public async Task<FacturaDto> CrearAsync(CrearFacturaRequest request, CancellationToken ct = default)
    {
        if (request.Letras.Count == 0)
            throw new BusinessRuleException("La factura debe tener al menos 1 letra.");
        if (await unitOfWork.Proveedores.GetByIdAsync(request.ProveedorId, ct) is null)
            throw new BusinessRuleException($"El proveedor con id '{request.ProveedorId}' no existe.");

        // Las letras ya vienen calculadas/editadas desde el frontend (generarLetras() en
        // app.js); acá solo validamos que la suma cuadre con el total, igual que ya
        // valida guardarFactura() del lado del cliente.
        var sumaLetras = request.Letras.Sum(l => l.Monto);
        if (Math.Abs(sumaLetras - request.MontoTotal) > 0.5m)
            throw new BusinessRuleException($"La suma de las letras ({sumaLetras:F2}) no coincide con el monto total ({request.MontoTotal:F2}).");

        var letras = request.Letras
            .Select(l => new Letra { Numero = (short)l.Numero, Monto = l.Monto, FechaVencimiento = l.FechaVencimiento, Pagada = false })
            .ToList();

        var factura = new Factura
        {
            NumeroFactura = request.NumeroFactura,
            ProveedorId = request.ProveedorId,
            Fecha = request.Fecha,
            MontoTotal = request.MontoTotal,
            Letras = letras
        };
        await unitOfWork.Facturas.AddAsync(factura, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await ObtenerAsync(factura.Id, ct);
    }

    public async Task<FacturaDto> ToggleLetraPagadaAsync(int facturaId, int numeroLetra, CancellationToken ct = default)
    {
        var factura = await unitOfWork.Facturas.GetByIdWithLetrasAsync(facturaId, ct) ?? throw new NotFoundException("Factura", facturaId);
        var letra = factura.Letras.FirstOrDefault(l => l.Numero == numeroLetra)
            ?? throw new NotFoundException("Letra", numeroLetra);

        letra.Pagada = !letra.Pagada;
        letra.FechaPago = letra.Pagada ? RelojNegocio.HoyPeru() : null;

        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(factura);
    }

    private static FacturaDto ToDto(Factura f) => new(
        f.Id, f.NumeroFactura, f.ProveedorId, f.Proveedor.Nombre, f.Fecha, f.MontoTotal,
        f.Letras.OrderBy(l => l.Numero).Select(l => new LetraDto(l.Numero, l.Monto, l.FechaVencimiento, l.Pagada, l.FechaPago)).ToList());
}
