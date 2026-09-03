using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

/// <summary>Una unidad física (un IMEI) de un <see cref="Producto"/>.</summary>
public class Equipo
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string Imei { get; set; } = string.Empty;
    public string EstadoFisico { get; set; } = "Nuevo";
    public decimal CostoCompra { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public int? ProveedorId { get; set; }
    public int? IngresoId { get; set; }
    public EstadoVenta EstadoVenta { get; set; } = EstadoVenta.Disponible;

    public Producto Producto { get; set; } = null!;
    public Proveedor? Proveedor { get; set; }
    public Ingreso? Ingreso { get; set; }
    public VentaItem? VentaItem { get; set; }
}
