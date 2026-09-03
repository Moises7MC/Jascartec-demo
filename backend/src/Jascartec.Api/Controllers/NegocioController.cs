using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/negocio")]
[Authorize]
public class NegocioController(INegocioService negocioService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NegocioDto>> Obtener(CancellationToken ct) =>
        Ok(await negocioService.ObtenerAsync(ct));

    [HttpPut]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<NegocioDto>> Actualizar(ActualizarNegocioRequest request, CancellationToken ct) =>
        Ok(await negocioService.ActualizarAsync(request, ct));
}
