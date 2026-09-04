namespace Jascartec.Domain.Entities;

/// <summary>Un equipo (IMEI) específico vendido dentro de una <see cref="Venta"/>.</summary>
public class VentaItem
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int EquipoId { get; set; }
    public decimal PrecioUnit { get; set; }

    // false cuando la venta que lo contiene fue anulada: libera el equipo_id para que pueda
    // volver a venderse (ver el índice único filtrado en VentaItemConfiguration).
    public bool Activo { get; set; } = true;

    public Venta Venta { get; set; } = null!;
    public Equipo Equipo { get; set; } = null!;
}
