namespace Jascartec.Application.Dtos;

public record NegocioDto(string RazonSocial, string Ruc, string Direccion, string Telefono, string Email, string? Web);

public record ActualizarNegocioRequest(string RazonSocial, string Ruc, string Direccion, string Telefono, string Email, string? Web);
