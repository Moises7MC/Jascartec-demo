namespace Jascartec.Application.Dtos;

public record UsuarioDto(int Id, string Usuario, string Nombre, string Rol, string Iniciales, bool Activo);

public record CrearUsuarioRequest(string Usuario, string Password, string Nombre, string Rol);

/// <summary>Password es opcional: si viene null/vacío, no se cambia la contraseña actual.</summary>
public record ActualizarUsuarioRequest(string Usuario, string? Password, string Nombre, string Rol, bool Activo);
