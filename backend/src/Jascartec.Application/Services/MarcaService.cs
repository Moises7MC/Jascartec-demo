using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Services;

public class MarcaService(IUnitOfWork unitOfWork) : IMarcaService
{
    public async Task<IReadOnlyList<MarcaDto>> ListarAsync(CancellationToken ct = default)
    {
        var marcas = await unitOfWork.Marcas.GetAllAsync(ct);
        return marcas.Select(m => new MarcaDto(m.Id, m.Nombre)).ToList();
    }

    public async Task<MarcaDto> CrearAsync(GuardarMarcaRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Marcas.ExisteNombreAsync(request.Nombre, ct: ct))
            throw new BusinessRuleException($"Ya existe una marca llamada '{request.Nombre}'.");

        var marca = new Marca { Nombre = request.Nombre };
        await unitOfWork.Marcas.AddAsync(marca, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new MarcaDto(marca.Id, marca.Nombre);
    }

    public async Task<MarcaDto> ActualizarAsync(int id, GuardarMarcaRequest request, CancellationToken ct = default)
    {
        var marca = await unitOfWork.Marcas.GetByIdAsync(id, ct) ?? throw new NotFoundException("Marca", id);
        if (await unitOfWork.Marcas.ExisteNombreAsync(request.Nombre, excluirId: id, ct: ct))
            throw new BusinessRuleException($"Ya existe una marca llamada '{request.Nombre}'.");

        marca.Nombre = request.Nombre;
        unitOfWork.Marcas.Update(marca);
        await unitOfWork.SaveChangesAsync(ct);
        return new MarcaDto(marca.Id, marca.Nombre);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var marca = await unitOfWork.Marcas.GetByIdAsync(id, ct) ?? throw new NotFoundException("Marca", id);
        if (await unitOfWork.Marcas.TieneProductosAsociadosAsync(id, ct))
            throw new BusinessRuleException("No se puede eliminar: la marca tiene productos asociados.");

        unitOfWork.Marcas.Remove(marca);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
