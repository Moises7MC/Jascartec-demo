using Jascartec.Application.Common;
using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Jascartec.Domain.Enums;

namespace Jascartec.Api.BackgroundServices;

/// <summary>
/// Abre automáticamente la caja de cada sucursal activa a las 00:00 (hora Perú) y la cierra
/// automáticamente a las 22:00, sin que nadie tenga que entrar a hacerlo — pero en cualquier
/// momento entre medio se puede seguir cerrando a mano (eso no cambia en nada).
///
/// Reutiliza ICajaService.AbrirAsync/CerrarAsync tal cual — así valida y calcula exactamente
/// igual que una apertura/cierre manual, no duplica esa lógica acá.
///
/// Dos decisiones que no venían especificadas y sí afectan el cuadre de caja:
/// - Monto inicial de la apertura automática: se usa el monto contado en el ÚLTIMO cierre de
///   esa misma sucursal (el vuelto que quedó físicamente en el cajón), no 0 — así el "efectivo
///   esperado" del nuevo día arranca realista. Si es la primera vez, arranca en 0.
/// - Monto contado del cierre automático: como a las 10pm no hay nadie ahí contando el
///   efectivo a mano, se usa el mismo "efectivo esperado" (diferencia 0) y se deja anotado en
///   observaciones que fue un cierre automático sin conteo real — así el Administrador sabe
///   que esa diferencia en particular no es un conteo verificado.
/// </summary>
public class CajaAutoService(IServiceScopeFactory scopeFactory, ILogger<CajaAutoService> logger) : BackgroundService
{
    private DateOnly? ultimaAperturaAutomatica;
    private DateOnly? ultimoCierreAutomatico;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                await RevisarAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error revisando la apertura/cierre automático de caja");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RevisarAsync(CancellationToken ct)
    {
        var ahora = RelojNegocio.AhoraPeru();
        var hoy = DateOnly.FromDateTime(ahora.DateTime);

        if (ahora.Hour == 0 && ultimaAperturaAutomatica != hoy)
        {
            await AbrirTodasAsync(ct);
            ultimaAperturaAutomatica = hoy;
        }

        if (ahora.Hour == 22 && ultimoCierreAutomatico != hoy)
        {
            await CerrarTodasAsync(ct);
            ultimoCierreAutomatico = hoy;
        }
    }

    private async Task AbrirTodasAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cajaService = scope.ServiceProvider.GetRequiredService<ICajaService>();

        var usuarioSistemaId = await ObtenerUsuarioSistemaIdAsync(unitOfWork, ct);
        if (usuarioSistemaId is null) return; // todavía no hay ningún Administrador registrado

        var sucursales = (await unitOfWork.Sucursales.GetAllAsync(ct)).Where(s => s.Activa);
        foreach (var sucursal in sucursales)
        {
            try
            {
                if (await cajaService.ObtenerAbiertaAsync(sucursal.Id, ct) is not null)
                    continue; // ya está abierta (la abrieron a mano antes de medianoche)

                var historial = await cajaService.ListarAsync(null, null, sucursal.Id, ct);
                var ultimaCerrada = historial
                    .Where(c => c.Estado == "Cerrada" && c.MontoContadoCierre.HasValue)
                    .OrderByDescending(c => c.Fecha).ThenByDescending(c => c.Id)
                    .FirstOrDefault();
                var montoInicial = ultimaCerrada?.MontoContadoCierre ?? 0m;

                await cajaService.AbrirAsync(
                    new AbrirCajaRequest(sucursal.Id, montoInicial, "Apertura automática (00:00) — monto inicial tomado del cierre anterior."),
                    usuarioSistemaId.Value, ct);
                logger.LogInformation("Caja abierta automáticamente en {Sucursal}", sucursal.Nombre);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo abrir automáticamente la caja de {Sucursal}", sucursal.Nombre);
            }
        }
    }

    private async Task CerrarTodasAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cajaService = scope.ServiceProvider.GetRequiredService<ICajaService>();

        var usuarioSistemaId = await ObtenerUsuarioSistemaIdAsync(unitOfWork, ct);
        if (usuarioSistemaId is null) return;

        var sucursales = (await unitOfWork.Sucursales.GetAllAsync(ct)).Where(s => s.Activa);
        foreach (var sucursal in sucursales)
        {
            try
            {
                var abierta = await cajaService.ObtenerAbiertaAsync(sucursal.Id, ct);
                if (abierta is null) continue; // ya la cerraron a mano, o nunca se abrió hoy

                await cajaService.CerrarAsync(abierta.Id,
                    new CerrarCajaRequest(abierta.EfectivoEsperado, "Cierre automático (10:00 pm) — no se contó el efectivo físico, se asumió igual al esperado."),
                    usuarioSistemaId.Value, ct);
                logger.LogInformation("Caja cerrada automáticamente en {Sucursal}", sucursal.Nombre);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo cerrar automáticamente la caja de {Sucursal}", sucursal.Nombre);
            }
        }
    }

    // Las aperturas/cierres automáticos necesitan quedar a nombre de algún usuario (la columna
    // no admite null) — se usa el Administrador activo más antiguo, y las observaciones dejan
    // bien claro que fue el sistema, no esa persona, quien lo hizo.
    private static async Task<int?> ObtenerUsuarioSistemaIdAsync(IUnitOfWork unitOfWork, CancellationToken ct)
    {
        var usuarios = await unitOfWork.Usuarios.GetAllAsync(ct);
        return usuarios
            .Where(u => u.Rol == RolUsuario.Administrador && u.Activo)
            .OrderBy(u => u.Id)
            .FirstOrDefault()?.Id;
    }
}
