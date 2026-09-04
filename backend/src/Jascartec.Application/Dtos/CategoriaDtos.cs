namespace Jascartec.Application.Dtos;

public record CategoriaDto(int Id, string Nombre, bool RequiereImei);

public record GuardarCategoriaRequest(string Nombre, bool RequiereImei);
