namespace Jascartec.Domain.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTimeOffset CreadoEn { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
