using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/facturas")]
[Authorize(Roles = "Administrador")]
public class FacturasController(IFacturaService facturaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FacturaDto>>> Listar(CancellationToken ct) =>
        Ok(await facturaService.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FacturaDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await facturaService.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<FacturaDto>> Crear(CrearFacturaRequest request, CancellationToken ct) =>
        Ok(await facturaService.CrearAsync(request, ct));

    [HttpPatch("{facturaId:int}/letras/{numeroLetra:int}/toggle-pagada")]
    public async Task<ActionResult<FacturaDto>> ToggleLetraPagada(int facturaId, int numeroLetra, CancellationToken ct) =>
        Ok(await facturaService.ToggleLetraPagadaAsync(facturaId, numeroLetra, ct));
}
