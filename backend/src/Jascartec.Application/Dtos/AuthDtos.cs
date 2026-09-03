namespace Jascartec.Application.Dtos;

public record LoginRequest(string Usuario, string Password);

public record LoginResponse(string Token, DateTimeOffset ExpiraEn, UsuarioDto Usuario);
