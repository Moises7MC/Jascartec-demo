using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

/// <summary>Una jornada de caja física: se abre con un monto inicial (el vuelto que se deja),
/// se cierra contando el efectivo real y comparándolo contra lo que el sistema esperaba. Una
/// por día — Fecha es la fecha de negocio (RelojNegocio.HoyPeru()) en la que se abrió.</summary>
public class CajaSesion
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public EstadoCajaSesion Estado { get; set; } = EstadoCajaSesion.Abierta;

    public int UsuarioAperturaId { get; set; }
    public DateTimeOffset AbiertaEn { get; set; }
    public decimal MontoInicial { get; set; }
    public string? ObservacionesApertura { get; set; }

    public int? UsuarioCierreId { get; set; }
    public DateTimeOffset? CerradaEn { get; set; }
    public decimal? MontoContadoCierre { get; set; }
    public string? ObservacionesCierre { get; set; }

    public Usuario UsuarioApertura { get; set; } = null!;
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<MovimientoCajaManual> Movimientos { get; set; } = new List<MovimientoCajaManual>();
}
