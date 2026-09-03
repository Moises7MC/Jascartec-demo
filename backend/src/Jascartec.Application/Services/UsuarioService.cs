using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;

namespace Jascartec.Application.Services;

public class UsuarioService(IUnitOfWork unitOfWork) : IUsuarioService
{
    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken ct = default)
    {
        var usuarios = await unitOfWork.Usuarios.GetAllAsync(ct);
        return usuarios.Select(ToDto).ToList();
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.Usuario, ct) is not null)
            throw new BusinessRuleException($"Ya existe un usuario con el nombre de usuario '{request.Usuario}'.");

        var usuario = new Usuario
        {
            NombreUsuario = request.Usuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Nombre = request.Nombre,
            Rol = ParsearRol(request.Rol),
            Iniciales = CalcularIniciales(request.Nombre),
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow
        };

        await unitOfWork.Usuarios.AddAsync(usuario, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(usuario);
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException("Usuario", id);

        var existente = await unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.Usuario, ct);
        if (existente is not null && existente.Id != id)
            throw new BusinessRuleException($"Ya existe un usuario con el nombre de usuario '{request.Usuario}'.");

        usuario.NombreUsuario = request.Usuario;
        usuario.Nombre = request.Nombre;
        usuario.Rol = ParsearRol(request.Rol);
        usuario.Iniciales = CalcularIniciales(request.Nombre);
        usuario.Activo = request.Activo;
        if (!string.IsNullOrWhiteSpace(request.Password))
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        unitOfWork.Usuarios.Update(usuario);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(usuario);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException("Usuario", id);
        unitOfWork.Usuarios.Remove(usuario);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static RolUsuario ParsearRol(string rol) =>
        Enum.TryParse<RolUsuario>(rol, ignoreCase: true, out var valor)
            ? valor
            : throw new BusinessRuleException($"Rol inválido: '{rol}'. Use 'Administrador' o 'Vendedor'.");

    private static string CalcularIniciales(string nombre) =>
        string.Concat(nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(p => char.ToUpperInvariant(p[0])));

    private static UsuarioDto ToDto(Usuario u) => new(u.Id, u.NombreUsuario, u.Nombre, u.Rol.ToString(), u.Iniciales, u.Activo);
}
