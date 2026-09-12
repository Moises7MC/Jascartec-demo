using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

/// <summary>Un ingreso o salida de efectivo anotado a mano dentro de una caja abierta
/// (ej. "compré cinta de embalaje, -S/20") — no viene de una venta o compra ya registrada.</summary>
public class MovimientoCajaManual
{
    public int Id { get; set; }
    public int CajaSesionId { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public TipoMovimientoCaja Tipo { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int UsuarioId { get; set; }

    public CajaSesion CajaSesion { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
