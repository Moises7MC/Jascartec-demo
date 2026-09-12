using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string Iniciales { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreadoEn { get; set; }
    // Null = ve todas las sucursales (típicamente el Administrador). Un Vendedor normalmente
    // tiene una sucursal asignada y solo vende/opera desde ahí.
    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
}
