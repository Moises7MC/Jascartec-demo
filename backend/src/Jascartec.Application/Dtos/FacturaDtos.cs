namespace Jascartec.Application.Dtos;

public record LetraDto(int Numero, decimal Monto, DateOnly FechaVencimiento, bool Pagada, DateOnly? FechaPago);

public record FacturaDto(int Id, string NumeroFactura, int ProveedorId, string Proveedor, DateOnly Fecha, decimal MontoTotal, IReadOnlyList<LetraDto> Letras);

public record CrearLetraRequest(int Numero, decimal Monto, DateOnly FechaVencimiento);

/// <summary>
/// Las letras vienen ya calculadas/editadas por el usuario (ver generarLetras() en app.js,
/// que reparte el monto en partes iguales y deja cada fila editable antes de guardar).
/// </summary>
public record CrearFacturaRequest(string NumeroFactura, int ProveedorId, DateOnly Fecha, decimal MontoTotal, IReadOnlyList<CrearLetraRequest> Letras);
