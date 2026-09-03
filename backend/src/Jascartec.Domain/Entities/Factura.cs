namespace Jascartec.Domain.Entities;

/// <summary>Factura de un proveedor (compra a crédito), pagada en letras/cuotas.</summary>
public class Factura
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int ProveedorId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal MontoTotal { get; set; }

    public Proveedor Proveedor { get; set; } = null!;
    public ICollection<Letra> Letras { get; set; } = new List<Letra>();
}
