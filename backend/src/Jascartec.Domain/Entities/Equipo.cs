using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

/// <summary>Una unidad física (un IMEI) de un <see cref="Producto"/>.</summary>
public class Equipo
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string Imei { get; set; } = string.Empty;
    // Los celulares dual SIM traen 2 IMEIs (uno por línea); este queda vacío en los que solo tienen uno.
    public string? Imei2 { get; set; }
    public string EstadoFisico { get; set; } = "Nuevo";
    public decimal CostoCompra { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public int? ProveedorId { get; set; }
    public int? IngresoId { get; set; }
    public EstadoVenta EstadoVenta { get; set; } = EstadoVenta.Disponible;
    // Dónde está físicamente este equipo ahora mismo. Se hereda del Ingreso al crearlo; una
    // venta solo puede tomar equipos de la sucursal donde se está vendiendo.
    public int SucursalId { get; set; }

    public Producto Producto { get; set; } = null!;
    public Proveedor? Proveedor { get; set; }
    public Ingreso? Ingreso { get; set; }
    public Sucursal Sucursal { get; set; } = null!;

    // Historial de veces que este equipo fue incluido en una venta; puede tener más de una
    // fila si una venta anterior fue anulada y el equipo se volvió a vender después.
    public ICollection<VentaItem> VentaItems { get; set; } = new List<VentaItem>();
}
