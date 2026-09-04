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
            throw new BusinessRuleException("La venta debe tener al menos un producto.");

        var formaPago = ParsearFormaPago(request.FormaPago);

        // Mismas reglas de negocio que hoy valida confirmarVenta() en app.js.
        if (formaPago == FormaPago.Credito && request.ClienteId is null)
            throw new BusinessRuleException("Para venta a crédito debe seleccionar un cliente registrado.");

        if (request.ClienteId is not null && await unitOfWork.Clientes.GetByIdAsync(request.ClienteId.Value, ct) is null)
            throw new BusinessRuleException($"El cliente con id '{request.ClienteId}' no existe.");

        // Resolvemos equipos/productos primero: necesitamos el Total de la venta antes de poder
        // calcular el recargo y el cronograma de cuotas. Cada línea es o un equipo puntual con IMEI
        // (categoría con serie individual) o N unidades de un producto por cantidad.
        var equiposUsadosEnEstaVenta = new HashSet<int>();
        var cantidadReservadaPorProducto = new Dictionary<int, int>();
        var itemsResueltos = new List<(Equipo? Equipo, Producto Producto, int Cantidad)>();
        foreach (var item in request.Items)
        {
            if (item.EquipoId is not null)
            {
                if (!equiposUsadosEnEstaVenta.Add(item.EquipoId.Value))
                    throw new BusinessRuleException($"El equipo con id '{item.EquipoId}' está repetido en la venta.");

                var equipo = await unitOfWork.Equipos.GetByIdAsync(item.EquipoId.Value, ct)
                    ?? throw new BusinessRuleException($"El equipo con id '{item.EquipoId}' no existe.");
                if (equipo.EstadoVenta != EstadoVenta.Disponible)
                    throw new BusinessRuleException($"El equipo con IMEI '{equipo.Imei}' ya no está disponible.");

                var producto = await unitOfWork.Productos.GetByIdAsync(equipo.ProductoId, ct)
                    ?? throw new BusinessRuleException($"El producto del equipo '{equipo.Imei}' no existe.");

                itemsResueltos.Add((equipo, producto, 1));
            }
            else if (item.ProductoId is not null)
            {
                if (item.Cantidad is null || item.Cantidad < 1)
                    throw new BusinessRuleException("Indique una cantidad válida (mayor a 0) para el producto.");

                var producto = await unitOfWork.Productos.GetByIdWithDetailsAsync(item.ProductoId.Value, ct)
                    ?? throw new BusinessRuleException($"El producto con id '{item.ProductoId}' no existe.");
                if (producto.Categoria.RequiereImei)
                    throw new BusinessRuleException($"El producto '{producto.Modelo}' se controla por IMEI: elija un equipo puntual, no una cantidad.");

                cantidadReservadaPorProducto.TryGetValue(producto.Id, out var yaReservado);
                var reservadoTotal = yaReservado + item.Cantidad.Value;
                if (reservadoTotal > producto.StockCantidad)
                    throw new BusinessRuleException($"Stock insuficiente de '{producto.Modelo}': disponible {producto.StockCantidad}, solicitado {reservadoTotal}.");
                cantidadReservadaPorProducto[producto.Id] = reservadoTotal;

                itemsResueltos.Add((null, producto, item.Cantidad.Value));
            }
            else
            {
                throw new BusinessRuleException("Cada línea de la venta debe traer un equipo (IMEI) o un producto con cantidad.");
            }
        }

        var total = itemsResueltos.Sum(x => x.Producto.Precio * x.Cantidad);
        var fecha = RelojNegocio.HoyPeru();

        decimal? montoInicial = null;
        var recargo = 0m;
        FrecuenciaPago? frecuencia = null;
        int? numCuotas = null;
        DateOnly? fechaPagoAcordada = null;

        if (formaPago == FormaPago.Credito)
        {
            if (request.MontoInicial is null || request.MontoInicial < 0)
                throw new BusinessRuleException("Ingrese el monto inicial (puede ser 0).");
            if (request.MontoInicial >= total)
                throw new BusinessRuleException("El monto inicial debe ser menor al total de la venta.");
            if (request.FrecuenciaPago is null)
                throw new BusinessRuleException("Seleccione la frecuencia de pago (semanal, quincenal o mensual).");

            frecuencia = ParsearFrecuencia(request.FrecuenciaPago);
            var maxCuotas = MaxCuotas(frecuencia.Value);
            if (request.NumCuotas is null || request.NumCuotas < 1 || request.NumCuotas > maxCuotas)
                throw new BusinessRuleException($"El número de cuotas para frecuencia {frecuencia} debe estar entre 1 y {maxCuotas} (máximo 2 meses).");

            montoInicial = request.MontoInicial.Value;
            numCuotas = request.NumCuotas.Value;
            recargo = CalcularRecargo(montoInicial.Value, total);

            var montoAFinanciar = (total - montoInicial.Value) + recargo;
            var (_, fechasCuotas) = CalcularPlanCuotas(fecha, frecuencia.Value, numCuotas.Value, montoAFinanciar);
            fechaPagoAcordada = fechasCuotas[^1];
        }

        var venta = new Venta
        {
            NumBoleta = await unitOfWork.Ventas.GenerarSiguienteNumBoletaAsync(ct),
            Fecha = fecha,
            ClienteId = request.ClienteId,
            FormaPago = formaPago,
            FechaPagoAcordada = fechaPagoAcordada,
            CreadoEn = DateTimeOffset.UtcNow,
            MontoInicial = montoInicial,
            Recargo = recargo,
            FrecuenciaPago = frecuencia,
            NumCuotas = numCuotas
        };
        await unitOfWork.Ventas.AddAsync(venta, ct);
        await unitOfWork.SaveChangesAsync(ct); // necesitamos el Id antes de crear los items

        foreach (var (equipo, producto, cantidad) in itemsResueltos)
        {
            if (equipo is not null)
            {
                equipo.EstadoVenta = EstadoVenta.Vendido;
                unitOfWork.Equipos.Update(equipo);
                venta.Items.Add(new VentaItem { VentaId = venta.Id, EquipoId = equipo.Id, Cantidad = 1, PrecioUnit = producto.Precio });
            }
            else
            {
                producto.StockCantidad -= cantidad;
                unitOfWork.Productos.Update(producto);
                venta.Items.Add(new VentaItem { VentaId = venta.Id, ProductoId = producto.Id, Cantidad = cantidad, PrecioUnit = producto.Precio });
            }

            await unitOfWork.SaveChangesAsync(ct); // aplica el UNIQUE(equipo_id) antes de seguir con el siguiente item
        }

        // El inicial cuenta como el primer abono, así entra al flujo de caja y al saldo desde el día uno.
        if (montoInicial is > 0)
        {
            venta.Abonos.Add(new Abono { VentaId = venta.Id, Fecha = fecha, Monto = montoInicial.Value });
            await unitOfWork.SaveChangesAsync(ct);
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
        // solo cambia el estado — así queda un rastro auditable. El stock vendido vuelve a
        // quedar disponible: el equipo puntual pasa a Disponible, o se devuelve la cantidad
        // al contador del producto, según el tipo de línea.
        foreach (var item in venta.Items)
        {
            if (item.EquipoId is not null)
            {
                var equipo = await unitOfWork.Equipos.GetByIdAsync(item.EquipoId.Value, ct);
                if (equipo is not null)
                {
                    equipo.EstadoVenta = EstadoVenta.Disponible;
                    unitOfWork.Equipos.Update(equipo);
                }
            }
            else if (item.ProductoId is not null)
            {
                var producto = await unitOfWork.Productos.GetByIdAsync(item.ProductoId.Value, ct);
                if (producto is not null)
                {
                    producto.StockCantidad += item.Cantidad;
                    unitOfWork.Productos.Update(producto);
                }
            }

            // Libera el equipo_id (índice único filtrado por activo=true) para que pueda
            // aparecer en una venta nueva; el item de la venta anulada queda como historial.
            item.Activo = false;
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

    private static FrecuenciaPago ParsearFrecuencia(string frecuencia) => frecuencia switch
    {
        "Semanal" => FrecuenciaPago.Semanal,
        "Quincenal" => FrecuenciaPago.Quincenal,
        "Mensual" => FrecuenciaPago.Mensual,
        _ => throw new BusinessRuleException($"Frecuencia de pago inválida: '{frecuencia}'. Use 'Semanal', 'Quincenal' o 'Mensual'.")
    };

    private static int MaxCuotas(FrecuenciaPago f) => f switch
    {
        FrecuenciaPago.Semanal => 8,   // 8 semanas ≈ 2 meses
        FrecuenciaPago.Quincenal => 4, // 4 quincenas = 2 meses
        FrecuenciaPago.Mensual => 2,   // 2 meses
        _ => 0
    };

    private static int IntervaloDias(FrecuenciaPago f) => f switch
    {
        FrecuenciaPago.Semanal => 7,
        FrecuenciaPago.Quincenal => 15,
        FrecuenciaPago.Mensual => 30,
        _ => 30
    };

    /// <summary>Tabla de recargo confirmada con el cliente: según si el inicial cubre el 50% o más
    /// del total, y si el total de la venta es mayor/igual o menor a S/1000.</summary>
    private static decimal CalcularRecargo(decimal montoInicial, decimal total)
    {
        var porcentajeInicial = total == 0 ? 0m : montoInicial / total;
        var inicialAlto = porcentajeInicial >= 0.5m;
        var precioAlto = total >= 1000m;

        if (inicialAlto) return precioAlto ? 100m : 50m;
        return precioAlto ? 200m : 100m;
    }

    /// <summary>Reparte el monto a financiar en cuotas iguales (la última absorbe el redondeo) espaciadas
    /// según la frecuencia, contadas desde la fecha de la venta.</summary>
    private static (List<decimal> Montos, List<DateOnly> Fechas) CalcularPlanCuotas(
        DateOnly fechaVenta, FrecuenciaPago frecuencia, int numCuotas, decimal montoAFinanciar)
    {
        var intervalo = IntervaloDias(frecuencia);
        var cuotaBase = Math.Round(montoAFinanciar / numCuotas, 2, MidpointRounding.AwayFromZero);
        var montos = new List<decimal>();
        var fechas = new List<DateOnly>();
        var acumulado = 0m;

        for (var i = 1; i <= numCuotas; i++)
        {
            var monto = i < numCuotas ? cuotaBase : montoAFinanciar - acumulado;
            montos.Add(monto);
            acumulado += monto;
            fechas.Add(fechaVenta.AddDays(intervalo * i));
        }

        return (montos, fechas);
    }

    /// <summary>El cronograma es 100% derivado (no se persiste): se recalcula igual que en CrearAsync y el
    /// estado "pagada" de cada cuota se infiere comparando lo abonado (sin contar el inicial) contra el plan.</summary>
    private static IReadOnlyList<CuotaCronogramaDto> ConstruirCronograma(Venta v)
    {
        if (v.FormaPago != FormaPago.Credito || v.FrecuenciaPago is null || v.NumCuotas is null || v.NumCuotas <= 0)
            return Array.Empty<CuotaCronogramaDto>();

        var montoInicial = v.MontoInicial ?? 0m;
        var montoAFinanciar = (v.Total - montoInicial) + v.Recargo;
        var (montos, fechas) = CalcularPlanCuotas(v.Fecha, v.FrecuenciaPago.Value, v.NumCuotas.Value, montoAFinanciar);

        var abonadoSinInicial = Math.Max(0m, v.MontoPagado - montoInicial);
        var acumulado = 0m;
        var cuotas = new List<CuotaCronogramaDto>();
        for (var i = 0; i < montos.Count; i++)
        {
            acumulado += montos[i];
            var pagada = abonadoSinInicial + 0.01m >= acumulado; // tolerancia de un centavo por redondeo
            cuotas.Add(new CuotaCronogramaDto(i + 1, montos[i], fechas[i], pagada));
        }

        return cuotas;
    }

    private static VentaDto ToDto(Venta v)
    {
        var items = v.Items.Select(i =>
        {
            // Línea de equipo puntual (IMEI) o línea por cantidad — cada una trae su propio producto.
            var producto = i.Equipo?.Producto ?? i.Producto!;
            return new VentaItemDto(
                i.EquipoId, producto.Id, producto.Marca.Nombre,
                $"{producto.Marca.Nombre} {producto.Modelo}", i.Equipo?.Imei, i.Cantidad, i.PrecioUnit);
        }).ToList();
        var abonos = v.Abonos.OrderBy(a => a.Fecha).Select(a => new AbonoDto(a.Id, a.Fecha, a.Monto)).ToList();
        var cuotas = ConstruirCronograma(v);

        return new VentaDto(
            v.Id, v.NumBoleta, v.Fecha, v.ClienteId,
            v.Cliente?.Nombre ?? "Cliente varios (sin registrar)", v.Cliente?.Documento, v.Cliente?.Direccion,
            v.FormaPago == FormaPago.Credito ? "Crédito" : "Contado", v.FechaPagoAcordada,
            items, abonos, v.Total, v.MontoPagado, v.SaldoPendiente,
            v.Estado.ToString(), v.FechaAnulacion, v.CreadoEn,
            v.MontoInicial, v.Recargo, v.FrecuenciaPago?.ToString(), v.NumCuotas, cuotas);
    }
}
