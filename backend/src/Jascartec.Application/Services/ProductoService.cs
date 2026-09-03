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

        var producto = new Producto
        {
            MarcaId = request.MarcaId,
            Modelo = request.Modelo,
            Almacenamiento = request.Almacenamiento,
            Ram = request.Ram,
            Color = request.Color,
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

        producto.MarcaId = request.MarcaId;
        producto.Modelo = request.Modelo;
        producto.Almacenamiento = request.Almacenamiento;
        producto.Ram = request.Ram;
        producto.Color = request.Color;
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
        var stock = await unitOfWork.Equipos.ContarDisponiblesPorProductoAsync(p.Id, ct);
        return new ProductoDto(
            p.Id, p.MarcaId, p.Marca.Nombre, p.Modelo, p.Almacenamiento, p.Ram, p.Color,
            p.Precio, p.CostoReferencial, p.ProveedorId, p.Proveedor?.Nombre, p.Codigo,
            p.Gama?.ToString(), p.ImagenUrl, stock);
    }
}
