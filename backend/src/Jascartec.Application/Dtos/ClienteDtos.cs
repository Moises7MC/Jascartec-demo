namespace Jascartec.Application.Dtos;

public record ClienteDto(int Id, string Nombre, string Documento, string Tipo, string? Contacto, string? Telefono, string? Email, string? Direccion);

public record GuardarClienteRequest(string Nombre, string Documento, string Tipo, string? Contacto, string? Telefono, string? Email, string? Direccion);
