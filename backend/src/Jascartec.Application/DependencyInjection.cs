using Jascartec.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jascartec.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INegocioService, NegocioService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IMarcaService, MarcaService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IIngresoService, IngresoService>();
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IVentaService, VentaService>();

        return services;
    }
}
