using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class ProductoService(IUnitOfWork unitOfWork) : IProductoService
{
    public async Task<IReadOnlyList<ProductoDto>> ListarAsync(CancellationToken ct = default)
    {
        var productos = await unitOfWork.Productos.GetAllWithDetailsAsync(ct);
        var resultado = new List<ProductoDto>(productos.Count);
        foreach (var p in productos)
            resultado.Add(await ToDtoAsync(p, ct));
        return resultado;
    }

    public async Task<ProductoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var producto = await unitOfWork.Productos.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Producto", id);
        return await ToDtoAsync(producto, ct);
    }

    public async Task<ProductoDto> CrearAsync(GuardarProductoRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Marcas.GetByIdAsync(request.MarcaId, ct) is null)
            throw new BusinessRuleException($"La marca con id '{request.MarcaId}' no existe.");
        if (await unitOfWork.Categorias.GetByIdAsync(request.CategoriaId, ct) is null)
            throw new BusinessRuleException($"La categoría con id '{request.CategoriaId}' no existe.");

        var producto = new Producto
        {
            CategoriaId = request.CategoriaId,
            MarcaId = request.MarcaId,
            Modelo = request.Modelo,
            Almacenamiento = request.Almacenamiento,
            Ram = request.Ram,
            Color = request.Color,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            CostoReferencial = request.CostoReferencial,
            ProveedorId = request.ProveedorId,
            Codigo = request.Codigo,
            Gama = ParsearGama(request.Gama),
            ImagenUrl = request.ImagenUrl,
            CreadoEn = DateTimeOffset.UtcNow
        };
        await unitOfWork.Productos.AddAsync(producto, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return await ObtenerAsync(producto.Id, ct);
    }

    public async Task<ProductoDto> ActualizarAsync(int id, GuardarProductoRequest request, CancellationToken ct = default)
    {
        var producto = await unitOfWork.Productos.GetByIdAsync(id, ct) ?? throw new NotFoundException("Producto", id);
        if (await unitOfWork.Marcas.GetByIdAsync(request.MarcaId, ct) is null)
            throw new BusinessRuleException($"La marca con id '{request.MarcaId}' no existe.");
        if (await unitOfWork.Categorias.GetByIdAsync(request.CategoriaId, ct) is null)
            throw new BusinessRuleException($"La categoría con id '{request.CategoriaId}' no existe.");

        producto.CategoriaId = request.CategoriaId;
        producto.MarcaId = request.MarcaId;
        producto.Modelo = request.Modelo;
        producto.Almacenamiento = request.Almacenamiento;
        producto.Ram = request.Ram;
        producto.Color = request.Color;
        producto.Descripcion = request.Descripcion;
        producto.Precio = request.Precio;
        producto.CostoReferencial = request.CostoReferencial;
        producto.ProveedorId = request.ProveedorId;
        producto.Codigo = request.Codigo;
        producto.Gama = ParsearGama(request.Gama);
        producto.ImagenUrl = request.ImagenUrl;

        unitOfWork.Productos.Update(producto);
        await unitOfWork.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var producto = await unitOfWork.Productos.GetByIdAsync(id, ct) ?? throw new NotFoundException("Producto", id);
        if (await unitOfWork.Equipos.ExisteAlgunoPorProductoAsync(id, ct))
            throw new BusinessRuleException("No se puede eliminar: el producto tiene equipos (IMEIs) registrados.");
        var stocks = await unitOfWork.ProductoStocks.GetAllAsync(ct);
        if (stocks.Any(s => s.ProductoId == id && s.Cantidad > 0))
            throw new BusinessRuleException("No se puede eliminar: el producto todavía tiene stock registrado en alguna sucursal.");

        unitOfWork.Productos.Remove(producto);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static Gama? ParsearGama(string? gama) =>
        string.IsNullOrWhiteSpace(gama)
            ? null
            : Enum.TryParse<Gama>(gama, ignoreCase: true, out var valor)
                ? valor
                : throw new BusinessRuleException($"Gama inválida: '{gama}'. Use 'Baja', 'Media' o 'Alta'.");

    private async Task<ProductoDto> ToDtoAsync(Producto p, CancellationToken ct)
    {
        // El stock se lleva distinto según el tipo de categoría: IMEI cuenta equipos individuales
        // agrupados por sucursal; cantidad simple lee las filas de ProductoStock (una por sucursal).
        List<StockSucursalDto> porSucursal;
        int total;
        if (p.Categoria.RequiereImei)
        {
            var agrupado = await unitOfWork.Equipos.ContarDisponiblesPorProductoAgrupadoPorSucursalAsync(p.Id, ct);
            var sucursales = await unitOfWork.Sucursales.GetAllAsync(ct);
            porSucursal = sucursales
                .Select(s => new StockSucursalDto(s.Id, s.Nombre, agrupado.GetValueOrDefault(s.Id)))
                .ToList();
            total = agrupado.Values.Sum();
        }
        else
        {
            porSucursal = p.Stocks.Select(s => new StockSucursalDto(s.SucursalId, s.Sucursal.Nombre, s.Cantidad)).ToList();
            total = p.Stocks.Sum(s => s.Cantidad);
        }

        return new ProductoDto(
            p.Id, p.CategoriaId, p.Categoria.Nombre, p.Categoria.RequiereImei, p.MarcaId, p.Marca.Nombre, p.Modelo,
            p.Almacenamiento, p.Ram, p.Color, p.Descripcion,
            p.Precio, p.CostoReferencial, p.ProveedorId, p.Proveedor?.Nombre, p.Codigo,
            p.Gama?.ToString(), p.ImagenUrl, total, porSucursal, p.CreadoEn);
    }
}
