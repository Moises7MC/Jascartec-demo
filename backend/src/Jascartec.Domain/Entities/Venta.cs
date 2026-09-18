using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public string NumBoleta { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public int? ClienteId { get; set; } // null = "cliente varios (sin registrar)"
    public int SucursalId { get; set; }
    // Quién atendió la venta — null en ventas viejas de antes de que existiera este campo.
    public int? UsuarioId { get; set; }
    public FormaPago FormaPago { get; set; } = FormaPago.Contado;
    // Solo tiene sentido en Contado (cómo se pagó la venta completa); en Crédito el medio de
    // pago real vive en cada Abono (incluido el inicial, que se guarda como el primer abono).
    public MedioPago? MedioPago { get; set; }
    public DateOnly? FechaPagoAcordada { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public EstadoBoleta Estado { get; set; } = EstadoBoleta.Activa;
    public DateOnly? FechaAnulacion { get; set; }

    // Solo aplican a ventas a crédito (FormaPago = Credito).
    public decimal? MontoInicial { get; set; }
    public decimal Recargo { get; set; } // cargo financiero aparte del precio del equipo; 0 en Contado
    public FrecuenciaPago? FrecuenciaPago { get; set; }
    public int? NumCuotas { get; set; }

    // Renovación de crédito: si este crédito nació absorbiendo el saldo pendiente de uno
    // anterior del mismo cliente (porque ya solo le faltaba la última cuota), acá queda cuánto
    // se le sumó y de cuál boleta viene — el crédito viejo se cierra con un abono automático
    // por ese mismo monto (ver VentaService.CrearAsync).
    public decimal SaldoAbsorbido { get; set; }
    public int? VentaRenovadaId { get; set; }

    public Cliente? Cliente { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public Usuario? Usuario { get; set; }
    public ICollection<VentaItem> Items { get; set; } = new List<VentaItem>();
    public ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public decimal Total => Items.Sum(i => i.PrecioUnit * i.Cantidad);
    public decimal MontoPagado => FormaPago == FormaPago.Contado ? Total : Abonos.Sum(a => a.Monto);
    public decimal SaldoPendiente => (Total + Recargo + SaldoAbsorbido) - MontoPagado;
}
