using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Services;

public class ProveedorService(IUnitOfWork unitOfWork) : IProveedorService
{
    public async Task<IReadOnlyList<ProveedorDto>> ListarAsync(CancellationToken ct = default)
    {
        var proveedores = await unitOfWork.Proveedores.GetAllAsync(ct);
        return proveedores.Select(ToDto).ToList();
    }

    public async Task<ProveedorDto> CrearAsync(GuardarProveedorRequest request, CancellationToken ct = default)
    {
        var proveedor = new Proveedor
        {
            Nombre = request.Nombre,
            Contacto = request.Contacto,
            Telefono = request.Telefono,
            Email = request.Email,
            Direccion = request.Direccion,
            CreadoEn = DateTimeOffset.UtcNow
        };
        await unitOfWork.Proveedores.AddAsync(proveedor, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(proveedor);
    }

    public async Task<ProveedorDto> ActualizarAsync(int id, GuardarProveedorRequest request, CancellationToken ct = default)
    {
        var proveedor = await unitOfWork.Proveedores.GetByIdAsync(id, ct) ?? throw new NotFoundException("Proveedor", id);
        proveedor.Nombre = request.Nombre;
        proveedor.Contacto = request.Contacto;
        proveedor.Telefono = request.Telefono;
        proveedor.Email = request.Email;
        proveedor.Direccion = request.Direccion;

        unitOfWork.Proveedores.Update(proveedor);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(proveedor);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var proveedor = await unitOfWork.Proveedores.GetByIdAsync(id, ct) ?? throw new NotFoundException("Proveedor", id);
        if (await unitOfWork.Proveedores.TieneVentasAsociadasAsync(id, ct))
            throw new BusinessRuleException("No se puede eliminar: el proveedor tiene movimientos asociados.");

        unitOfWork.Proveedores.Remove(proveedor);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static ProveedorDto ToDto(Proveedor p) => new(p.Id, p.Nombre, p.Contacto, p.Telefono, p.Email, p.Direccion);
}
