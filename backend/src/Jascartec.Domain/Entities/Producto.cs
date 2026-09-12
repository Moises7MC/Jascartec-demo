using Jascartec.Domain.Enums;

namespace Jascartec.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public int MarcaId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string? Almacenamiento { get; set; }
    public string? Ram { get; set; }
    public string? Color { get; set; }
    // Detalle libre para productos de categorías sin specs de celular (p.ej. "Bluetooth 5.0, autonomía 20h").
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoReferencial { get; set; }
    public int? ProveedorId { get; set; }
    public string? Codigo { get; set; }
    public Gama? Gama { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTimeOffset CreadoEn { get; set; }

    public Categoria Categoria { get; set; } = null!;
    public Marca Marca { get; set; } = null!;
    public Proveedor? Proveedor { get; set; }
    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
    // Solo tiene sentido cuando Categoria.RequiereImei = false: el stock por sucursal se lleva
    // acá (una fila por sucursal) en vez de contar Equipos individuales.
    public ICollection<ProductoStock> Stocks { get; set; } = new List<ProductoStock>();
}
