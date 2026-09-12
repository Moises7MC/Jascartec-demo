using Jascartec.Application.Abstractions;
using Jascartec.Application.Common;
using Jascartec.Infrastructure.Auth;
using Jascartec.Infrastructure.Persistence;
using Jascartec.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jascartec.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<JascartecDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("JascartecDb")));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<INegocioRepository, NegocioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IMarcaRepository, MarcaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IIngresoRepository, IngresoRepository>();
        services.AddScoped<IRepository<Jascartec.Domain.Entities.IngresoItem>, Repository<Jascartec.Domain.Entities.IngresoItem>>();
        services.AddScoped<IEquipoRepository, EquipoRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ICajaSesionRepository, CajaSesionRepository>();
        services.AddScoped<IRepository<Jascartec.Domain.Entities.MovimientoCajaManual>, Repository<Jascartec.Domain.Entities.MovimientoCajaManual>>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
