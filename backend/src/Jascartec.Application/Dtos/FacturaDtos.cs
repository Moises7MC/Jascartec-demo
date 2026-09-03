namespace Jascartec.Application.Dtos;

public record LetraDto(int Numero, decimal Monto, DateOnly FechaVencimiento, bool Pagada, DateOnly? FechaPago);

public record FacturaDto(int Id, string NumeroFactura, int ProveedorId, string Proveedor, DateOnly Fecha, decimal MontoTotal, IReadOnlyList<LetraDto> Letras);

public record CrearFacturaRequest(string NumeroFactura, int ProveedorId, DateOnly Fecha, decimal MontoTotal, int NumeroLetras);
