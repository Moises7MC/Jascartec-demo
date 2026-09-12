using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

/// <summary>Una jornada de caja física: se abre con un monto inicial (el vuelto que se deja),
/// se cierra contando el efectivo real y comparándolo contra lo que el sistema esperaba. Una
/// por día POR SUCURSAL — Fecha es la fecha de negocio (RelojNegocio.HoyPeru()) en la que se
/// abrió. Cada sucursal tiene su propia caja física, independiente de las demás.</summary>
public class CajaSesion
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public int SucursalId { get; set; }
    public EstadoCajaSesion Estado { get; set; } = EstadoCajaSesion.Abierta;

    public int UsuarioAperturaId { get; set; }
    public DateTimeOffset AbiertaEn { get; set; }
    public decimal MontoInicial { get; set; }
    public string? ObservacionesApertura { get; set; }

    public int? UsuarioCierreId { get; set; }
    public DateTimeOffset? CerradaEn { get; set; }
    public decimal? MontoContadoCierre { get; set; }
    public string? ObservacionesCierre { get; set; }

    public Sucursal Sucursal { get; set; } = null!;
    public Usuario UsuarioApertura { get; set; } = null!;
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<MovimientoCajaManual> Movimientos { get; set; } = new List<MovimientoCajaManual>();
}
