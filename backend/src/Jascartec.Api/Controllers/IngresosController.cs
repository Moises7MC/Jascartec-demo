using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/ingresos")]
[Authorize(Roles = "Administrador")]
public class IngresosController(IIngresoService ingresoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngresoDto>>> Listar(CancellationToken ct) =>
        Ok(await ingresoService.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IngresoDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await ingresoService.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<IngresoDto>> Crear(CrearIngresoRequest request, CancellationToken ct) =>
        Ok(await ingresoService.CrearAsync(request, ct));
}
