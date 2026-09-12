namespace Jascartec.Application.Dtos;

/// <summary>SucursalId nulo = ve/opera todas las sucursales (típicamente el Administrador).</summary>
public record UsuarioDto(int Id, string Usuario, string Nombre, string Rol, string Iniciales, bool Activo, int? SucursalId, string? Sucursal);

public record CrearUsuarioRequest(string Usuario, string Password, string Nombre, string Rol, int? SucursalId = null);

/// <summary>Password es opcional: si viene null/vacío, no se cambia la contraseña actual.</summary>
public record ActualizarUsuarioRequest(string Usuario, string? Password, string Nombre, string Rol, bool Activo, int? SucursalId = null);
