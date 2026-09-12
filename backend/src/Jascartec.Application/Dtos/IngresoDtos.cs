namespace Jascartec.Application.Dtos;

/// <summary>Imei2 es opcional: se usa cuando el equipo es un celular dual SIM y trae un segundo IMEI.</summary>
public record EquipoDto(int Id, int ProductoId, string Producto, string Imei, string? Imei2, string EstadoFisico, decimal CostoCompra, DateOnly FechaIngreso, string EstadoVenta, int SucursalId, string Sucursal);

/// <summary>Una línea de compra por cantidad (sin IMEI) — productos de una categoría que no requiere serie individual.</summary>
public record IngresoItemDto(int Id, int ProductoId, string Producto, int Cantidad, decimal CostoUnit);

public record IngresoDto(int Id, DateOnly Fecha, int ProveedorId, string Proveedor, string? NumeroFactura, int SucursalId, string Sucursal, IReadOnlyList<EquipoDto> Equipos, IReadOnlyList<IngresoItemDto> Items, DateTimeOffset CreadoEn);

/// <summary>Una línea del ingreso: si el producto es de una categoría con IMEI, se llena Imei (y opcionalmente
/// Imei2 si es dual SIM; Cantidad se ignora, es siempre 1); si no, se llena Cantidad (Imei/Imei2 deben venir vacíos).</summary>
public record IngresoLineaRequest(int ProductoId, string? Imei, string? Imei2, int? Cantidad, decimal CostoUnit);

public record CrearIngresoRequest(DateOnly Fecha, int ProveedorId, string? NumeroFactura, int SucursalId, IReadOnlyList<IngresoLineaRequest> Items);
