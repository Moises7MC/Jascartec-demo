namespace Jascartec.Application.Dtos;

public record EquipoDto(int Id, int ProductoId, string Producto, string Imei, string EstadoFisico, decimal CostoCompra, DateOnly FechaIngreso, string EstadoVenta);

/// <summary>Una línea de compra por cantidad (sin IMEI) — productos de una categoría que no requiere serie individual.</summary>
public record IngresoItemDto(int Id, int ProductoId, string Producto, int Cantidad, decimal CostoUnit);

public record IngresoDto(int Id, DateOnly Fecha, int ProveedorId, string Proveedor, string? NumeroFactura, IReadOnlyList<EquipoDto> Equipos, IReadOnlyList<IngresoItemDto> Items, DateTimeOffset CreadoEn);

/// <summary>Una línea del ingreso: si el producto es de una categoría con IMEI, se llena Imei (Cantidad se
/// ignora, es siempre 1); si no, se llena Cantidad (Imei debe venir vacío).</summary>
public record IngresoLineaRequest(int ProductoId, string? Imei, int? Cantidad, decimal CostoUnit);

public record CrearIngresoRequest(DateOnly Fecha, int ProveedorId, string? NumeroFactura, IReadOnlyList<IngresoLineaRequest> Items);
