using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Services;

public class CategoriaService(IUnitOfWork unitOfWork) : ICategoriaService
{
    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken ct = default)
    {
        var categorias = await unitOfWork.Categorias.GetAllAsync(ct);
        return categorias.Select(c => new CategoriaDto(c.Id, c.Nombre, c.RequiereImei)).ToList();
    }

    public async Task<CategoriaDto> CrearAsync(GuardarCategoriaRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Categorias.ExisteNombreAsync(request.Nombre, ct: ct))
            throw new BusinessRuleException($"Ya existe una categoría llamada '{request.Nombre}'.");

        var categoria = new Categoria { Nombre = request.Nombre, RequiereImei = request.RequiereImei };
        await unitOfWork.Categorias.AddAsync(categoria, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.RequiereImei);
    }

    public async Task<CategoriaDto> ActualizarAsync(int id, GuardarCategoriaRequest request, CancellationToken ct = default)
    {
        var categoria = await unitOfWork.Categorias.GetByIdAsync(id, ct) ?? throw new NotFoundException("Categoría", id);
        if (await unitOfWork.Categorias.ExisteNombreAsync(request.Nombre, excluirId: id, ct: ct))
            throw new BusinessRuleException($"Ya existe una categoría llamada '{request.Nombre}'.");

        // Cambiar RequiereImei con productos ya cargados dejaría el inventario existente en un
        // estado ambiguo (¿sus equipos IMEI siguen valiendo? ¿su StockCantidad arranca en 0?),
        // así que solo se permite mientras la categoría no tenga productos asociados.
        if (categoria.RequiereImei != request.RequiereImei && await unitOfWork.Categorias.TieneProductosAsociadosAsync(id, ct))
            throw new BusinessRuleException("No se puede cambiar el tipo de control de stock: la categoría ya tiene productos asociados.");

        categoria.Nombre = request.Nombre;
        categoria.RequiereImei = request.RequiereImei;
        unitOfWork.Categorias.Update(categoria);
        await unitOfWork.SaveChangesAsync(ct);
        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.RequiereImei);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var categoria = await unitOfWork.Categorias.GetByIdAsync(id, ct) ?? throw new NotFoundException("Categoría", id);
        if (await unitOfWork.Categorias.TieneProductosAsociadosAsync(id, ct))
            throw new BusinessRuleException("No se puede eliminar: la categoría tiene productos asociados.");

        unitOfWork.Categorias.Remove(categoria);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
