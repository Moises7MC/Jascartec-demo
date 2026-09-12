using System.Security.Claims;
using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize] // Administrador y Vendedor abren/cierran caja por igual
public class CajaController(ICajaService cajaService) : ControllerBase
{
    private int UsuarioActualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("abierta")]
    public async Task<ActionResult<CajaSesionDto?>> ObtenerAbierta(CancellationToken ct) =>
        Ok(await cajaService.ObtenerAbiertaAsync(ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CajaSesionDto>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        Ok(await cajaService.ListarAsync(desde, hasta, ct));

    [HttpPost("abrir")]
    public async Task<ActionResult<CajaSesionDto>> Abrir(AbrirCajaRequest request, CancellationToken ct) =>
        Ok(await cajaService.AbrirAsync(request, UsuarioActualId, ct));

    [HttpPost("{id:int}/cerrar")]
    public async Task<ActionResult<CajaSesionDto>> Cerrar(int id, CerrarCajaRequest request, CancellationToken ct) =>
        Ok(await cajaService.CerrarAsync(id, request, UsuarioActualId, ct));

    [HttpPost("{id:int}/movimientos")]
    public async Task<ActionResult<CajaSesionDto>> RegistrarMovimiento(int id, CrearMovimientoCajaRequest request, CancellationToken ct) =>
        Ok(await cajaService.RegistrarMovimientoAsync(id, request, UsuarioActualId, ct));
}
