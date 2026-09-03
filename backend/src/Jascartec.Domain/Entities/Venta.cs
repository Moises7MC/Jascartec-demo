using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public string NumBoleta { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public int? ClienteId { get; set; } // null = "cliente varios (sin registrar)"
    public FormaPago FormaPago { get; set; } = FormaPago.Contado;
    public DateOnly? FechaPagoAcordada { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public EstadoBoleta Estado { get; set; } = EstadoBoleta.Activa;
    public DateOnly? FechaAnulacion { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<VentaItem> Items { get; set; } = new List<VentaItem>();
    public ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public decimal Total => Items.Sum(i => i.PrecioUnit);
    public decimal MontoPagado => FormaPago == FormaPago.Contado ? Total : Abonos.Sum(a => a.Monto);
    public decimal SaldoPendiente => Total - MontoPagado;
}
