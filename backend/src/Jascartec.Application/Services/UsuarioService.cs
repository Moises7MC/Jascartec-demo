using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class UsuarioService(IUnitOfWork unitOfWork) : IUsuarioService
{
    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken ct = default)
    {
        var usuarios = await unitOfWork.Usuarios.GetAllWithDetailsAsync(ct);
        return usuarios.Select(ToDto).ToList();
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.Usuario, ct) is not null)
            throw new BusinessRuleException($"Ya existe un usuario con el nombre de usuario '{request.Usuario}'.");
        if (request.SucursalId is not null && await unitOfWork.Sucursales.GetByIdAsync(request.SucursalId.Value, ct) is null)
            throw new BusinessRuleException($"La sucursal con id '{request.SucursalId}' no existe.");

        var usuario = new Usuario
        {
            NombreUsuario = request.Usuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Nombre = request.Nombre,
            Rol = ParsearRol(request.Rol),
            Iniciales = CalcularIniciales(request.Nombre),
            Activo = true,
            SucursalId = request.SucursalId,
            CreadoEn = DateTimeOffset.UtcNow
        };

        await unitOfWork.Usuarios.AddAsync(usuario, ct);
        await unitOfWork.SaveChangesAsync(ct);
        var recargado = await unitOfWork.Usuarios.GetByNombreUsuarioAsync(usuario.NombreUsuario, ct) ?? usuario;
        return ToDto(recargado);
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException("Usuario", id);

        var existente = await unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.Usuario, ct);
        if (existente is not null && existente.Id != id)
            throw new BusinessRuleException($"Ya existe un usuario con el nombre de usuario '{request.Usuario}'.");
        if (request.SucursalId is not null && await unitOfWork.Sucursales.GetByIdAsync(request.SucursalId.Value, ct) is null)
            throw new BusinessRuleException($"La sucursal con id '{request.SucursalId}' no existe.");

        var nuevoRol = ParsearRol(request.Rol);
        if (usuario.Rol == RolUsuario.Administrador && nuevoRol != RolUsuario.Administrador && await EsUnicoAdministradorAsync(usuario.Id, ct))
            throw new BusinessRuleException("Debe quedar al menos un Administrador en el sistema.");

        usuario.NombreUsuario = request.Usuario;
        usuario.Nombre = request.Nombre;
        usuario.Rol = nuevoRol;
        usuario.Iniciales = CalcularIniciales(request.Nombre);
        usuario.Activo = request.Activo;
        usuario.SucursalId = request.SucursalId;
        if (!string.IsNullOrWhiteSpace(request.Password))
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        unitOfWork.Usuarios.Update(usuario);
        await unitOfWork.SaveChangesAsync(ct);
        var recargado = await unitOfWork.Usuarios.GetByNombreUsuarioAsync(usuario.NombreUsuario, ct) ?? usuario;
        return ToDto(recargado);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException("Usuario", id);
        if (usuario.Rol == RolUsuario.Administrador && await EsUnicoAdministradorAsync(usuario.Id, ct))
            throw new BusinessRuleException("Debe quedar al menos un Administrador en el sistema.");

        unitOfWork.Usuarios.Remove(usuario);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>¿"usuarioId" es el único Administrador que queda registrado?</summary>
    private async Task<bool> EsUnicoAdministradorAsync(int usuarioId, CancellationToken ct)
    {
        var todos = await unitOfWork.Usuarios.GetAllAsync(ct);
        return todos.Count(u => u.Rol == RolUsuario.Administrador) == 1
            && todos.Any(u => u.Id == usuarioId && u.Rol == RolUsuario.Administrador);
    }

    private static RolUsuario ParsearRol(string rol) =>
        Enum.TryParse<RolUsuario>(rol, ignoreCase: true, out var valor)
            ? valor
            : throw new BusinessRuleException($"Rol inválido: '{rol}'. Use 'Administrador' o 'Vendedor'.");

    private static string CalcularIniciales(string nombre) =>
        string.Concat(nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(p => char.ToUpperInvariant(p[0])));

    private static UsuarioDto ToDto(Usuario u) => new(u.Id, u.NombreUsuario, u.Nombre, u.Rol.ToString(), u.Iniciales, u.Activo, u.SucursalId, u.Sucursal?.Nombre);
}
