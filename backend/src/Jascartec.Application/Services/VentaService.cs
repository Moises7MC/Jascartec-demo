using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class VentaService(IUnitOfWork unitOfWork) : IVentaService
{
    public async Task<IReadOnlyList<VentaDto>> ListarAsync(CancellationToken ct = default)
    {
        var ventas = await unitOfWork.Ventas.GetAllWithDetailsAsync(ct);
        return ventas.Select(ToDto).ToList();
    }

    public async Task<VentaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var venta = await unitOfWork.Ventas.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Venta", id);
        return ToDto(venta);
    }

    public async Task<VentaDto> CrearAsync(CrearVentaRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new BusinessRuleException("La venta debe tener al menos un equipo.");

        var formaPago = ParsearFormaPago(request.FormaPago);

        // Mismas reglas de negocio que hoy valida confirmarVenta() en app.js.
        if (formaPago == FormaPago.Credito && request.ClienteId is null)
            throw new BusinessRuleException("Para venta a crédito debe seleccionar un cliente registrado.");
        if (formaPago == FormaPago.Credito && request.FechaPagoAcordada is null)
            throw new BusinessRuleException("Ingrese la fecha de pago acordada con el cliente.");

        if (request.ClienteId is not null && await unitOfWork.Clientes.GetByIdAsync(request.ClienteId.Value, ct) is null)
            throw new BusinessRuleException($"El cliente con id '{request.ClienteId}' no existe.");

        var venta = new Venta
        {
            NumBoleta = await unitOfWork.Ventas.GenerarSiguienteNumBoletaAsync(ct),
            Fecha = RelojNegocio.HoyPeru(),
            ClienteId = request.ClienteId,
            FormaPago = formaPago,
            FechaPagoAcordada = formaPago == FormaPago.Credito ? request.FechaPagoAcordada : null,
            CreadoEn = DateTimeOffset.UtcNow
        };
        await unitOfWork.Ventas.AddAsync(venta, ct);
        await unitOfWork.SaveChangesAsync(ct); // necesitamos el Id antes de crear los items

        var equiposUsadosEnEstaVenta = new HashSet<int>();
        foreach (var item in request.Items)
        {
            if (!equiposUsadosEnEstaVenta.Add(item.EquipoId))
                throw new BusinessRuleException($"El equipo con id '{item.EquipoId}' está repetido en la venta.");

            var equipo = await unitOfWork.Equipos.GetByIdAsync(item.EquipoId, ct)
                ?? throw new BusinessRuleException($"El equipo con id '{item.EquipoId}' no existe.");
            if (equipo.EstadoVenta != EstadoVenta.Disponible)
                throw new BusinessRuleException($"El equipo con IMEI '{equipo.Imei}' ya no está disponible.");

            var producto = await unitOfWork.Productos.GetByIdAsync(equipo.ProductoId, ct)
                ?? throw new BusinessRuleException($"El producto del equipo '{equipo.Imei}' no existe.");

            equipo.EstadoVenta = EstadoVenta.Vendido;
            unitOfWork.Equipos.Update(equipo);
            venta.Items.Add(new VentaItem { VentaId = venta.Id, EquipoId = equipo.Id, PrecioUnit = producto.Precio });

            await unitOfWork.SaveChangesAsync(ct); // aplica el UNIQUE(equipo_id) antes de seguir con el siguiente item
        }

        return await ObtenerAsync(venta.Id, ct);
    }

    public async Task<VentaDto> RegistrarAbonoAsync(int ventaId, RegistrarAbonoRequest request, CancellationToken ct = default)
    {
        if (request.Monto <= 0)
            throw new BusinessRuleException("El monto del abono debe ser mayor a 0.");

        var venta = await unitOfWork.Ventas.GetByIdWithDetailsAsync(ventaId, ct) ?? throw new NotFoundException("Venta", ventaId);
        if (venta.Estado == EstadoBoleta.Anulada)
            throw new BusinessRuleException("Esta venta está anulada; no se le pueden registrar abonos.");
        if (venta.FormaPago != FormaPago.Credito)
            throw new BusinessRuleException("Solo se pueden registrar abonos en ventas a crédito.");
        if (request.Monto > venta.SaldoPendiente)
            throw new BusinessRuleException($"El abono ({request.Monto:F2}) supera el saldo pendiente ({venta.SaldoPendiente:F2}).");

        venta.Abonos.Add(new Abono { VentaId = ventaId, Fecha = request.Fecha, Monto = request.Monto });
        await unitOfWork.SaveChangesAsync(ct);

        return await ObtenerAsync(ventaId, ct);
    }

    public async Task<VentaDto> AnularAsync(int id, CancellationToken ct = default)
    {
        var venta = await unitOfWork.Ventas.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Venta", id);
        if (venta.Estado == EstadoBoleta.Anulada)
            throw new BusinessRuleException("Esta venta ya está anulada.");
        if (venta.Abonos.Count > 0)
            throw new BusinessRuleException("No se puede anular: esta venta ya tiene abonos registrados. Gestione la devolución del dinero por separado antes de anular.");

        // El correlativo (num_boleta) y el registro de la venta se conservan tal cual,
        // solo cambia el estado — así queda un rastro auditable. Los equipos vendidos
        // vuelven a quedar disponibles para venderse de nuevo.
        foreach (var item in venta.Items)
        {
            var equipo = await unitOfWork.Equipos.GetByIdAsync(item.EquipoId, ct);
            if (equipo is not null)
            {
                equipo.EstadoVenta = EstadoVenta.Disponible;
                unitOfWork.Equipos.Update(equipo);
            }
        }

        venta.Estado = EstadoBoleta.Anulada;
        venta.FechaAnulacion = RelojNegocio.HoyPeru();
        await unitOfWork.SaveChangesAsync(ct);

        return await ObtenerAsync(id, ct);
    }

    private static FormaPago ParsearFormaPago(string formaPago) => formaPago switch
    {
        "Contado" => FormaPago.Contado,
        "Crédito" or "Credito" => FormaPago.Credito,
        _ => throw new BusinessRuleException($"Forma de pago inválida: '{formaPago}'. Use 'Contado' o 'Crédito'.")
    };

    private static VentaDto ToDto(Venta v)
    {
        var items = v.Items.Select(i => new VentaItemDto(
            i.EquipoId, i.Equipo.ProductoId, i.Equipo.Producto.Marca.Nombre,
            $"{i.Equipo.Producto.Marca.Nombre} {i.Equipo.Producto.Modelo}", i.Equipo.Imei, i.PrecioUnit)).ToList();
        var abonos = v.Abonos.OrderBy(a => a.Fecha).Select(a => new AbonoDto(a.Id, a.Fecha, a.Monto)).ToList();

        return new VentaDto(
            v.Id, v.NumBoleta, v.Fecha, v.ClienteId,
            v.Cliente?.Nombre ?? "Cliente varios (sin registrar)", v.Cliente?.Documento, v.Cliente?.Direccion,
            v.FormaPago == FormaPago.Credito ? "Crédito" : "Contado", v.FechaPagoAcordada,
            items, abonos, v.Total, v.MontoPagado, v.SaldoPendiente,
            v.Estado.ToString(), v.FechaAnulacion, v.CreadoEn);
    }
}
