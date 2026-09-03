namespace Jascartec.Domain.Enums;

/// <summary>Estado de una venta/boleta. Al anular, la boleta y su correlativo se
/// conservan (no se borra ni se reutiliza el número), solo cambia el estado.</summary>
public enum EstadoBoleta
{
    Activa,
    Anulada
}
