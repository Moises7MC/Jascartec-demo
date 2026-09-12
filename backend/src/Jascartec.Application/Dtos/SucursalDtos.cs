namespace Jascartec.Application.Dtos;

public record SucursalDto(int Id, string Nombre, bool Activa);

public record GuardarSucursalRequest(string Nombre, bool Activa = true);
