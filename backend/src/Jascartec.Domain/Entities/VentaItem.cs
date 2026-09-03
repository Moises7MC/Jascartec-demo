namespace Jascartec.Domain.Entities;

/// <summary>Un equipo (IMEI) específico vendido dentro de una <see cref="Venta"/>.</summary>
public class VentaItem
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int EquipoId { get; set; }
    public decimal PrecioUnit { get; set; }

    public Venta Venta { get; set; } = null!;
    public Equipo Equipo { get; set; } = null!;
}
