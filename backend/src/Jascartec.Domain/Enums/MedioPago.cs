namespace Jascartec.Domain.Enums;

/// <summary>Cómo entró físicamente el dinero de un pago (venta al contado o abono). Solo
/// "Efectivo" afecta el cuadre de caja física — Yape/Tarjeta/Transferencia van a una
/// cuenta bancaria, no al cajón de dinero.</summary>
public enum MedioPago
{
    Efectivo,
    Yape,
    Tarjeta,
    Transferencia
}
