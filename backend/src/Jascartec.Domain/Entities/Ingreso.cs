namespace Jascartec.Domain.Entities;

/// <summary>Una compra a un proveedor; trae uno o más <see cref="Equipo"/> (IMEIs).</summary>
public class Ingreso
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public int ProveedorId { get; set; }
    public string? NumeroFactura { get; set; }
    // Momento exacto en que se registró (distinto de Fecha, que es la fecha de compra que
    // escribe el vendedor) — para poder mostrar la hora real en el listado, como en Venta.
    public DateTimeOffset CreadoEn { get; set; }
    // A qué sucursal entra esta mercadería — define la sucursal de los Equipos que se crean, y
    // a qué contador de ProductoStock se le suma en las líneas por cantidad.
    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;
    public Proveedor Proveedor { get; set; } = null!;
    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
    // Líneas por cantidad (sin IMEI) de productos de categorías que no requieren serie individual.
    public ICollection<IngresoItem> Items { get; set; } = new List<IngresoItem>();
}
