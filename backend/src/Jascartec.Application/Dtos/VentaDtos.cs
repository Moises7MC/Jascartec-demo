namespace Jascartec.Application.Dtos;

/// <summary>EquipoId/Imei son nulos en una línea por cantidad (producto de categoría sin IMEI); Cantidad
/// es siempre 1 en una línea de equipo puntual.</summary>
public record VentaItemDto(int? EquipoId, int ProductoId, string Marca, string Producto, string? Imei, string? Imei2, int Cantidad, decimal PrecioUnit);

public record AbonoDto(int Id, DateOnly Fecha, decimal Monto, string MedioPago);

/// <summary>Una cuota del plan de pagos de una venta a crédito. "Pagada" se calcula comparando lo abonado
/// (sin contar el inicial) acumulado contra el monto acumulado del plan hasta esa cuota — no se persiste.</summary>
public record CuotaCronogramaDto(int Numero, decimal Monto, DateOnly FechaVencimiento, bool Pagada);

public record VentaDto(
    int Id, string NumBoleta, DateOnly Fecha, int? ClienteId, string Cliente, string? ClienteDocumento,
    string? ClienteDireccion, string FormaPago, string? MedioPago, DateOnly? FechaPagoAcordada,
    IReadOnlyList<VentaItemDto> Items, IReadOnlyList<AbonoDto> Abonos,
    decimal Total, decimal MontoPagado, decimal SaldoPendiente,
    string Estado, DateOnly? FechaAnulacion, DateTimeOffset CreadoEn,
    decimal? MontoInicial, decimal Recargo, string? FrecuenciaPago, int? NumCuotas,
    IReadOnlyList<CuotaCronogramaDto> Cuotas);

/// <summary>Una línea del carrito de venta: o bien un equipo puntual con IMEI (EquipoId, GET
/// /api/equipos/disponibles), o bien N unidades de un producto por cantidad (ProductoId + Cantidad) —
/// exactamente uno de los dos según si la categoría del producto requiere IMEI.</summary>
public record CrearVentaItemRequest(int? EquipoId, int? ProductoId, int? Cantidad);

/// <summary>MontoInicial, FrecuenciaPago y NumCuotas solo aplican (y son obligatorios) cuando FormaPago="Crédito";
/// el servicio calcula Recargo y FechaPagoAcordada, no se envían desde el cliente. MedioPago es obligatorio
/// si FormaPago="Contado", o si es "Crédito" con un MontoInicial mayor a 0 (describe cómo entró ese pago).</summary>
public record CrearVentaRequest(
    int? ClienteId, IReadOnlyList<CrearVentaItemRequest> Items, string FormaPago, string? MedioPago = null,
    decimal? MontoInicial = null, string? FrecuenciaPago = null, int? NumCuotas = null);

public record RegistrarAbonoRequest(DateOnly Fecha, decimal Monto, string MedioPago);
