namespace Jascartec.Application.Dtos;

public record MarcaDto(int Id, string Nombre);

public record GuardarMarcaRequest(string Nombre);
