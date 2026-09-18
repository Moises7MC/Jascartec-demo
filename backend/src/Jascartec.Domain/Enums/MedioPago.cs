namespace Jascartec.Domain.Enums;

/// <summary>Cómo entró físicamente el dinero de un pago (venta al contado o abono). Solo
/// "Efectivo" afecta el cuadre de caja física — Yape/Tarjeta/Transferencia van a una
/// cuenta bancaria, no al cajón de dinero. "Renovacion" no es dinero real: es el abono
/// automático que cierra un crédito viejo cuando su saldo se absorbe dentro de uno nuevo
/// (por eso tampoco debe contar en el cuadre de caja).</summary>
public enum MedioPago
{
    Efectivo,
    Yape,
    Tarjeta,
    Transferencia,
    Renovacion
}
