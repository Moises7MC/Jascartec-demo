using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize] // Administrador y Vendedor registran ventas por igual
public class VentasController(IVentaService ventaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VentaDto>>> Listar(CancellationToken ct) =>
        Ok(await ventaService.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await ventaService.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Crear(CrearVentaRequest request, CancellationToken ct) =>
        Ok(await ventaService.CrearAsync(request, ct));

    [HttpPost("{id:int}/abonos")]
    public async Task<ActionResult<VentaDto>> RegistrarAbono(int id, RegistrarAbonoRequest request, CancellationToken ct) =>
        Ok(await ventaService.RegistrarAbonoAsync(id, request, ct));

    [HttpPost("{id:int}/anular")]
    public async Task<ActionResult<VentaDto>> Anular(int id, CancellationToken ct) =>
        Ok(await ventaService.AnularAsync(id, ct));
}
