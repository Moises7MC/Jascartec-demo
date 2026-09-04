namespace Jascartec.Domain.Entities;

/// <summary>Un tipo de producto (Celulares, Accesorios, Impresoras, etc.). El administrador la crea y
/// decide si sus productos se controlan por IMEI/serie individual (<see cref="Equipo"/>) o por
/// cantidad simple en stock (<see cref="Producto.StockCantidad"/>).</summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool RequiereImei { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
