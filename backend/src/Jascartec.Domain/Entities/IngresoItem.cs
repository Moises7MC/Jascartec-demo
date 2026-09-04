namespace Jascartec.Domain.Entities;

/// <summary>Una línea de compra por cantidad (sin IMEI) dentro de un <see cref="Ingreso"/> — para
/// productos de una categoría que no requiere serie individual (accesorios, carcasas, etc.).
/// Un mismo Ingreso puede traer tanto <see cref="Equipo"/> (líneas con IMEI) como IngresoItem.</summary>
public class IngresoItem
{
    public int Id { get; set; }
    public int IngresoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnit { get; set; }

    public Ingreso Ingreso { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
