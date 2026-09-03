using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public int MarcaId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string? Almacenamiento { get; set; }
    public string? Ram { get; set; }
    public string? Color { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoReferencial { get; set; }
    public int? ProveedorId { get; set; }
    public string? Codigo { get; set; }
    public Gama? Gama { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTimeOffset CreadoEn { get; set; }

    public Marca Marca { get; set; } = null!;
    public Proveedor? Proveedor { get; set; }
    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
}
