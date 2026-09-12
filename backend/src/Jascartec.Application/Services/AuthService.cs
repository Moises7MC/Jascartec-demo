using Jascartec.Application.Abstractions;
using Jascartec.Application.Common;
using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public class AuthService(IUnitOfWork unitOfWork, IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.Usuario, ct);
        if (usuario is null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new InvalidCredentialsException();

        var (token, expiraEn) = jwtTokenGenerator.Generar(usuario);
        var usuarioDto = new UsuarioDto(usuario.Id, usuario.NombreUsuario, usuario.Nombre, usuario.Rol.ToString(), usuario.Iniciales, usuario.Activo, usuario.SucursalId, usuario.Sucursal?.Nombre);
        return new LoginResponse(token, expiraEn, usuarioDto);
    }
}
