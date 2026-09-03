namespace Jascartec.Domain.Entities;

/// <summary>Un pago parcial de una <see cref="Venta"/> a crédito.</summary>
public class Abono
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal Monto { get; set; }

    public Venta Venta { get; set; } = null!;
}
