namespace Jascartec.Application.Dtos;

public record ProductoDto(
    int Id, int MarcaId, string Marca, string Modelo, string? Almacenamiento, string? Ram, string? Color,
    decimal Precio, decimal? CostoReferencial, int? ProveedorId, string? Proveedor, string? Codigo,
    string? Gama, string? ImagenUrl, int StockDisponible);

public record GuardarProductoRequest(
    int MarcaId, string Modelo, string? Almacenamiento, string? Ram, string? Color,
    decimal Precio, decimal? CostoReferencial, int? ProveedorId, string? Codigo, string? Gama, string? ImagenUrl);
