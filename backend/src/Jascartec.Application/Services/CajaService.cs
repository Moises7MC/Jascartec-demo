using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class CajaService(IUnitOfWork unitOfWork) : ICajaService
{
    public async Task<CajaSesionDto?> ObtenerAbiertaAsync(CancellationToken ct = default)
    {
        var sesion = await unitOfWork.CajaSesiones.GetAbiertaAsync(ct);
        return sesion is null ? null : await ToDtoAsync(sesion, ct);
    }

    public async Task<IReadOnlyList<CajaSesionDto>> ListarAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var sesiones = await unitOfWork.CajaSesiones.GetAllWithDetailsAsync(ct);
        var filtradas = sesiones.Where(s => (!desde.HasValue || s.Fecha >= desde) && (!hasta.HasValue || s.Fecha <= hasta));
        var dtos = new List<CajaSesionDto>();
        foreach (var s in filtradas) dtos.Add(await ToDtoAsync(s, ct));
        return dtos;
    }

    public async Task<CajaSesionDto> AbrirAsync(AbrirCajaRequest request, int usuarioId, CancellationToken ct = default)
    {
        if (request.MontoInicial < 0)
            throw new BusinessRuleException("El monto inicial no puede ser negativo.");

        if (await unitOfWork.CajaSesiones.GetAbiertaAsync(ct) is not null)
            throw new BusinessRuleException("Ya hay una caja abierta. Ciérrala antes de abrir una nueva.");

        var hoy = RelojNegocio.HoyPeru();
        if (await unitOfWork.CajaSesiones.GetByFechaAsync(hoy, ct) is not null)
            throw new BusinessRuleException("La caja de hoy ya se abrió (y se cerró) antes. Solo se permite una caja por día.");

        var sesion = new CajaSesion
        {
            Fecha = hoy,
            Estado = EstadoCajaSesion.Abierta,
            UsuarioAperturaId = usuarioId,
            AbiertaEn = DateTimeOffset.UtcNow,
            MontoInicial = request.MontoInicial,
            ObservacionesApertura = request.Observaciones
        };
        await unitOfWork.CajaSesiones.AddAsync(sesion, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var recargada = await unitOfWork.CajaSesiones.GetByIdWithDetailsAsync(sesion.Id, ct) ?? sesion;
        return await ToDtoAsync(recargada, ct);
    }

    public async Task<CajaSesionDto> CerrarAsync(int id, CerrarCajaRequest request, int usuarioId, CancellationToken ct = default)
    {
        if (request.MontoContadoCierre < 0)
            throw new BusinessRuleException("El monto contado no puede ser negativo.");

        var sesion = await unitOfWork.CajaSesiones.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Sesión de caja", id);
        if (sesion.Estado != EstadoCajaSesion.Abierta)
            throw new BusinessRuleException("Esta caja ya está cerrada.");

        sesion.MontoContadoCierre = request.MontoContadoCierre;
        sesion.ObservacionesCierre = request.Observaciones;
        sesion.CerradaEn = DateTimeOffset.UtcNow;
        sesion.UsuarioCierreId = usuarioId;
        sesion.Estado = EstadoCajaSesion.Cerrada;
        unitOfWork.CajaSesiones.Update(sesion);
        await unitOfWork.SaveChangesAsync(ct);

        return await ToDtoAsync(sesion, ct);
    }

    public async Task<CajaSesionDto> RegistrarMovimientoAsync(int cajaSesionId, CrearMovimientoCajaRequest request, int usuarioId, CancellationToken ct = default)
    {
        if (request.Monto <= 0)
            throw new BusinessRuleException("El monto del movimiento debe ser mayor a 0.");
        if (string.IsNullOrWhiteSpace(request.Concepto))
            throw new BusinessRuleException("Indique el concepto del movimiento.");

        var tipo = request.Tipo switch
        {
            "Ingreso" => TipoMovimientoCaja.Ingreso,
            "Salida" => TipoMovimientoCaja.Salida,
            _ => throw new BusinessRuleException($"Tipo de movimiento inválido: '{request.Tipo}'. Use 'Ingreso' o 'Salida'.")
        };

        var sesion = await unitOfWork.CajaSesiones.GetByIdWithDetailsAsync(cajaSesionId, ct) ?? throw new NotFoundException("Sesión de caja", cajaSesionId);
        if (sesion.Estado != EstadoCajaSesion.Abierta)
            throw new BusinessRuleException("No se pueden agregar movimientos a una caja ya cerrada.");

        await unitOfWork.MovimientosCaja.AddAsync(new MovimientoCajaManual
        {
            CajaSesionId = cajaSesionId,
            CreadoEn = DateTimeOffset.UtcNow,
            Tipo = tipo,
            Concepto = request.Concepto.Trim(),
            Monto = request.Monto,
            UsuarioId = usuarioId
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var recargada = await unitOfWork.CajaSesiones.GetByIdWithDetailsAsync(cajaSesionId, ct) ?? sesion;
        return await ToDtoAsync(recargada, ct);
    }

    // Solo el efectivo mueve la caja física: ventas al contado en efectivo, y abonos (o el
    // inicial de un crédito, que se guarda como el primer abono) pagados en efectivo, ambos
    // fechados el mismo día que la sesión. Yape/Tarjeta/Transferencia no tocan el cajón.
    private async Task<decimal> CalcularVentasEfectivoAsync(DateOnly fecha, CancellationToken ct)
    {
        var ventas = await unitOfWork.Ventas.GetAllWithDetailsAsync(ct);
        decimal total = 0m;
        foreach (var v in ventas.Where(v => v.Estado == Domain.Enums.EstadoBoleta.Activa))
        {
            if (v.FormaPago == Domain.Enums.FormaPago.Contado && v.Fecha == fecha && v.MedioPago == MedioPago.Efectivo)
                total += v.Total;

            total += v.Abonos.Where(a => a.Fecha == fecha && a.MedioPago == MedioPago.Efectivo).Sum(a => a.Monto);
        }
        return total;
    }

    private async Task<CajaSesionDto> ToDtoAsync(CajaSesion s, CancellationToken ct)
    {
        var ventasEfectivo = await CalcularVentasEfectivoAsync(s.Fecha, ct);
        var ingresos = s.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto);
        var salidas = s.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Salida).Sum(m => m.Monto);
        var efectivoEsperado = s.MontoInicial + ventasEfectivo + ingresos - salidas;
        decimal? diferencia = s.Estado == EstadoCajaSesion.Cerrada && s.MontoContadoCierre.HasValue
            ? s.MontoContadoCierre.Value - efectivoEsperado
            : null;

        var movimientos = s.Movimientos.OrderBy(m => m.CreadoEn)
            .Select(m => new MovimientoCajaDto(m.Id, m.CreadoEn, m.Tipo.ToString(), m.Concepto, m.Monto, m.Usuario.Nombre))
            .ToList();

        return new CajaSesionDto(
            s.Id, s.Fecha, s.Estado.ToString(),
            s.UsuarioApertura.Nombre, s.AbiertaEn, s.MontoInicial, s.ObservacionesApertura,
            s.UsuarioCierre?.Nombre, s.CerradaEn, s.MontoContadoCierre, s.ObservacionesCierre,
            ventasEfectivo, ingresos, salidas, efectivoEsperado, diferencia, movimientos);
    }
}
