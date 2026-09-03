namespace Jascartec.Application.Dtos;

public record VentaItemDto(int EquipoId, string Producto, string Imei, decimal PrecioUnit);

public record AbonoDto(int Id, DateOnly Fecha, decimal Monto);

public record VentaDto(
    int Id, string NumBoleta, DateOnly Fecha, int? ClienteId, string Cliente, string? ClienteDocumento,
    string? ClienteDireccion, string FormaPago, DateOnly? FechaPagoAcordada,
    IReadOnlyList<VentaItemDto> Items, IReadOnlyList<AbonoDto> Abonos,
    decimal Total, decimal MontoPagado, decimal SaldoPendiente);

/// <summary>Un item de venta se identifica por el ProductoId; el servicio elige el primer IMEI disponible.</summary>
public record CrearVentaItemRequest(int ProductoId);

public record CrearVentaRequest(int? ClienteId, IReadOnlyList<CrearVentaItemRequest> Items, string FormaPago, DateOnly? FechaPagoAcordada);

public record RegistrarAbonoRequest(DateOnly Fecha, decimal Monto);
