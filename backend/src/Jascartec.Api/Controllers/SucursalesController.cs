using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/sucursales")]
[Authorize] // ambos roles necesitan la lista (para elegir sucursal al vender/ingresar); solo Administrador escribe
public class SucursalesController(ISucursalService sucursalService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SucursalDto>>> Listar(CancellationToken ct) =>
        Ok(await sucursalService.ListarAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<SucursalDto>> Crear(GuardarSucursalRequest request, CancellationToken ct) =>
        Ok(await sucursalService.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<SucursalDto>> Actualizar(int id, GuardarSucursalRequest request, CancellationToken ct) =>
        Ok(await sucursalService.ActualizarAsync(id, request, ct));
}
