namespace Jascartec.Application.Dtos;

public record EquipoDto(int Id, int ProductoId, string Producto, string Imei, string EstadoFisico, decimal CostoCompra, string EstadoVenta);

public record IngresoDto(int Id, DateOnly Fecha, int ProveedorId, string Proveedor, string? NumeroFactura, IReadOnlyList<EquipoDto> Equipos);

public record IngresoItemRequest(int ProductoId, string Imei, decimal CostoUnit);

public record CrearIngresoRequest(DateOnly Fecha, int ProveedorId, string? NumeroFactura, IReadOnlyList<IngresoItemRequest> Items);
