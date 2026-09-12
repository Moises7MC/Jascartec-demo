namespace Jascartec.Application.Dtos;

public record StockSucursalDto(int SucursalId, string Sucursal, int Cantidad);

/// <summary>StockDisponible sigue siendo el total sumando todas las sucursales (compatibilidad
/// con lo que ya leía el frontend); StockPorSucursal trae el desglose para saber "en cuál
/// sucursal está" cada cosa.</summary>
public record ProductoDto(
    int Id, int CategoriaId, string Categoria, bool RequiereImei, int MarcaId, string Marca, string Modelo,
    string? Almacenamiento, string? Ram, string? Color, string? Descripcion,
    decimal Precio, decimal? CostoReferencial, int? ProveedorId, string? Proveedor, string? Codigo,
    string? Gama, string? ImagenUrl, int StockDisponible, IReadOnlyList<StockSucursalDto> StockPorSucursal,
    DateTimeOffset CreadoEn);

public record GuardarProductoRequest(
    int CategoriaId, int MarcaId, string Modelo, string? Almacenamiento, string? Ram, string? Color, string? Descripcion,
    decimal Precio, decimal? CostoReferencial, int? ProveedorId, string? Codigo, string? Gama, string? ImagenUrl);
