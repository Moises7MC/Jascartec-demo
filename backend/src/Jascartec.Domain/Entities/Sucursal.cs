namespace Jascartec.Domain.Entities;

/// <summary>Una tienda física del negocio. Todo lo que existe físicamente (equipos, stock por
/// cantidad, ventas, caja) pertenece a una sucursal — así el dueño puede ver el negocio junto
/// o separado por local.</summary>
public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}
