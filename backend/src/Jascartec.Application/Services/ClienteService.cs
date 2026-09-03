using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class ClienteService(IUnitOfWork unitOfWork) : IClienteService
{
    public async Task<IReadOnlyList<ClienteDto>> ListarAsync(CancellationToken ct = default)
    {
        var clientes = await unitOfWork.Clientes.GetAllAsync(ct);
        return clientes.Select(ToDto).ToList();
    }

    public async Task<ClienteDto> CrearAsync(GuardarClienteRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Clientes.ExisteDocumentoAsync(request.Documento, ct: ct))
            throw new BusinessRuleException($"Ya existe un cliente con el documento '{request.Documento}'.");

        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            Documento = request.Documento,
            Tipo = ParsearTipo(request.Tipo),
            Contacto = request.Contacto,
            Telefono = request.Telefono,
            Email = request.Email,
            Direccion = request.Direccion,
            CreadoEn = DateTimeOffset.UtcNow
        };
        await unitOfWork.Clientes.AddAsync(cliente, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(cliente);
    }

    public async Task<ClienteDto> ActualizarAsync(int id, GuardarClienteRequest request, CancellationToken ct = default)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id, ct) ?? throw new NotFoundException("Cliente", id);
        if (await unitOfWork.Clientes.ExisteDocumentoAsync(request.Documento, excluirId: id, ct: ct))
            throw new BusinessRuleException($"Ya existe un cliente con el documento '{request.Documento}'.");

        cliente.Nombre = request.Nombre;
        cliente.Documento = request.Documento;
        cliente.Tipo = ParsearTipo(request.Tipo);
        cliente.Contacto = request.Contacto;
        cliente.Telefono = request.Telefono;
        cliente.Email = request.Email;
        cliente.Direccion = request.Direccion;

        unitOfWork.Clientes.Update(cliente);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(cliente);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id, ct) ?? throw new NotFoundException("Cliente", id);
        if (await unitOfWork.Clientes.TieneVentasAsociadasAsync(id, ct))
            throw new BusinessRuleException("No se puede eliminar: el cliente tiene ventas registradas.");

        unitOfWork.Clientes.Remove(cliente);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static TipoCliente ParsearTipo(string tipo) =>
        Enum.TryParse<TipoCliente>(tipo, ignoreCase: true, out var valor)
            ? valor
            : throw new BusinessRuleException($"Tipo de cliente inválido: '{tipo}'. Use 'Particular' o 'Empresa'.");

    private static ClienteDto ToDto(Cliente c) =>
        new(c.Id, c.Nombre, c.Documento, c.Tipo.ToString(), c.Contacto, c.Telefono, c.Email, c.Direccion);
}
