namespace Jascartec.Application.Dtos;

public record ProductoDto(
    int Id, int CategoriaId, string Categoria, bool RequiereImei, int MarcaId, string Marca, string Modelo,
    string? Almacenamiento, string? Ram, string? Color, string? Descripcion,
    decimal Precio, decimal? CostoReferencial, int? ProveedorId, string? Proveedor, string? Codigo,
    string? Gama, string? ImagenUrl, int StockDisponible, DateTimeOffset CreadoEn);

public record GuardarProductoRequest(
    int CategoriaId, int MarcaId, string Modelo, string? Almacenamiento, string? Ram, string? Color, string? Descripcion,
    decimal Precio, decimal? CostoReferencial, int? ProveedorId, string? Codigo, string? Gama, string? ImagenUrl);
