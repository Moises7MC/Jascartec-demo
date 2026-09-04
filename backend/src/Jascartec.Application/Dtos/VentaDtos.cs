namespace Jascartec.Application.Dtos;

public record VentaItemDto(int EquipoId, int ProductoId, string Marca, string Producto, string Imei, decimal PrecioUnit);

public record AbonoDto(int Id, DateOnly Fecha, decimal Monto);

/// <summary>Una cuota del plan de pagos de una venta a crédito. "Pagada" se calcula comparando lo abonado
/// (sin contar el inicial) acumulado contra el monto acumulado del plan hasta esa cuota — no se persiste.</summary>
public record CuotaCronogramaDto(int Numero, decimal Monto, DateOnly FechaVencimiento, bool Pagada);

public record VentaDto(
    int Id, string NumBoleta, DateOnly Fecha, int? ClienteId, string Cliente, string? ClienteDocumento,
    string? ClienteDireccion, string FormaPago, DateOnly? FechaPagoAcordada,
    IReadOnlyList<VentaItemDto> Items, IReadOnlyList<AbonoDto> Abonos,
    decimal Total, decimal MontoPagado, decimal SaldoPendiente,
    string Estado, DateOnly? FechaAnulacion, DateTimeOffset CreadoEn,
    decimal? MontoInicial, decimal Recargo, string? FrecuenciaPago, int? NumCuotas,
    IReadOnlyList<CuotaCronogramaDto> Cuotas);

/// <summary>El vendedor elige el IMEI puntual en el carrito (GET /api/equipos/disponibles), no solo el modelo.</summary>
public record CrearVentaItemRequest(int EquipoId);

/// <summary>MontoInicial, FrecuenciaPago y NumCuotas solo aplican (y son obligatorios) cuando FormaPago="Crédito";
/// el servicio calcula Recargo y FechaPagoAcordada, no se envían desde el cliente.</summary>
public record CrearVentaRequest(
    int? ClienteId, IReadOnlyList<CrearVentaItemRequest> Items, string FormaPago,
    decimal? MontoInicial = null, string? FrecuenciaPago = null, int? NumCuotas = null);

public record RegistrarAbonoRequest(DateOnly Fecha, decimal Monto);
