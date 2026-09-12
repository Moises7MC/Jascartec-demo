using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Services;

// No hay EliminarAsync a propósito: casi todo el sistema (equipos, ventas, caja, usuarios)
// referencia una sucursal — en vez de borrarla (y arriesgar romper el historial), se desactiva
// con "Activa = false" para que deje de aparecer como opción al registrar algo nuevo.
public class SucursalService(IUnitOfWork unitOfWork) : ISucursalService
{
    public async Task<IReadOnlyList<SucursalDto>> ListarAsync(CancellationToken ct = default)
    {
        var sucursales = await unitOfWork.Sucursales.GetAllAsync(ct);
        return sucursales.OrderBy(s => s.Id).Select(ToDto).ToList();
    }

    public async Task<SucursalDto> CrearAsync(GuardarSucursalRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new BusinessRuleException("El nombre de la sucursal es obligatorio.");

        var sucursal = new Sucursal { Nombre = request.Nombre.Trim(), Activa = request.Activa };
        await unitOfWork.Sucursales.AddAsync(sucursal, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(sucursal);
    }

    public async Task<SucursalDto> ActualizarAsync(int id, GuardarSucursalRequest request, CancellationToken ct = default)
    {
        var sucursal = await unitOfWork.Sucursales.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sucursal", id);
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new BusinessRuleException("El nombre de la sucursal es obligatorio.");

        sucursal.Nombre = request.Nombre.Trim();
        sucursal.Activa = request.Activa;
        unitOfWork.Sucursales.Update(sucursal);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(sucursal);
    }

    private static SucursalDto ToDto(Sucursal s) => new(s.Id, s.Nombre, s.Activa);
}
