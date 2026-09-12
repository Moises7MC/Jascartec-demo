namespace Jascartec.Domain.Entities;

/// <summary>Cuánto stock hay de un producto por cantidad (sin IMEI) en cada sucursal. Reemplaza
/// al viejo Producto.StockCantidad (que era un único total global) — ahora cada sucursal lleva
/// su propio contador. Clave compuesta (ProductoId, SucursalId).</summary>
public class ProductoStock
{
    public int ProductoId { get; set; }
    public int SucursalId { get; set; }
    public int Cantidad { get; set; }

    public Producto Producto { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
}
