namespace Jascartec.Application.Dtos;

public record ProveedorDto(int Id, string Nombre, string? Contacto, string? Telefono, string? Email, string? Direccion);

public record GuardarProveedorRequest(string Nombre, string? Contacto, string? Telefono, string? Email, string? Direccion);
