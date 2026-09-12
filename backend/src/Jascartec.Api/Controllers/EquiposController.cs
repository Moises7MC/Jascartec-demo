using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/equipos")]
[Authorize] // Administrador y Vendedor arman ventas por igual
public class EquiposController(IEquipoService equipoService) : ControllerBase
{
    [HttpGet("disponibles")]
    public async Task<ActionResult<IReadOnlyList<EquipoDto>>> ListarDisponibles([FromQuery] int productoId, [FromQuery] int? sucursalId, CancellationToken ct) =>
        Ok(await equipoService.ListarDisponiblesAsync(productoId, sucursalId, ct));
}
