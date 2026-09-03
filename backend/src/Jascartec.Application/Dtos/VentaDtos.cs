namespace Jascartec.Application.Dtos;

public record VentaItemDto(int EquipoId, int ProductoId, string Marca, string Producto, string Imei, decimal PrecioUnit);

public record AbonoDto(int Id, DateOnly Fecha, decimal Monto);

public record VentaDto(
    int Id, string NumBoleta, DateOnly Fecha, int? ClienteId, string Cliente, string? ClienteDocumento,
    string? ClienteDireccion, string FormaPago, DateOnly? FechaPagoAcordada,
    IReadOnlyList<VentaItemDto> Items, IReadOnlyList<AbonoDto> Abonos,
    decimal Total, decimal MontoPagado, decimal SaldoPendiente,
    string Estado, DateOnly? FechaAnulacion);

/// <summary>El vendedor elige el IMEI puntual en el carrito (GET /api/equipos/disponibles), no solo el modelo.</summary>
public record CrearVentaItemRequest(int EquipoId);

public record CrearVentaRequest(int? ClienteId, IReadOnlyList<CrearVentaItemRequest> Items, string FormaPago, DateOnly? FechaPagoAcordada);

public record RegistrarAbonoRequest(DateOnly Fecha, decimal Monto);
