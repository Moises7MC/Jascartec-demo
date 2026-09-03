namespace Jascartec.Domain.Entities;

/// <summary>Una cuota de pago de una <see cref="Factura"/>.</summary>
public class Letra
{
    public int Id { get; set; }
    public int FacturaId { get; set; }
    public short Numero { get; set; }
    public decimal Monto { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool Pagada { get; set; }
    public DateOnly? FechaPago { get; set; }

    public Factura Factura { get; set; } = null!;
}
