namespace Jascartec.Domain.Entities;

/// <summary>Una compra a un proveedor; trae uno o más <see cref="Equipo"/> (IMEIs).</summary>
public class Ingreso
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public int ProveedorId { get; set; }
    public string? NumeroFactura { get; set; }

    public Proveedor Proveedor { get; set; } = null!;
    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
    // Líneas por cantidad (sin IMEI) de productos de categorías que no requieren serie individual.
    public ICollection<IngresoItem> Items { get; set; } = new List<IngresoItem>();
}
