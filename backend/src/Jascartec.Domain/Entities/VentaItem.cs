namespace Jascartec.Domain.Entities;

/// <summary>Una línea vendida dentro de una <see cref="Venta"/>: o bien un equipo (IMEI) específico
/// (<see cref="EquipoId"/> no nulo, <see cref="Cantidad"/> siempre 1), o bien N unidades de un producto
/// de una categoría por cantidad (<see cref="ProductoId"/> no nulo, sin equipo puntual).</summary>
public class VentaItem
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int? EquipoId { get; set; }
    public int? ProductoId { get; set; } // solo se usa cuando EquipoId es null; si hay equipo, su ProductoId ya se conoce vía Equipo
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnit { get; set; } // precio unitario; el importe de la línea es PrecioUnit * Cantidad

    // false cuando la venta que lo contiene fue anulada: libera el equipo_id para que pueda
    // volver a venderse (ver el índice único filtrado en VentaItemConfiguration).
    public bool Activo { get; set; } = true;

    public Venta Venta { get; set; } = null!;
    public Equipo? Equipo { get; set; }
    public Producto? Producto { get; set; }
}
