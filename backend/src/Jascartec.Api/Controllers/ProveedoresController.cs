using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize(Roles = "Administrador")]
public class ProveedoresController(IProveedorService proveedorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorDto>>> Listar(CancellationToken ct) =>
        Ok(await proveedorService.ListarAsync(ct));

    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> Crear(GuardarProveedorRequest request, CancellationToken ct) =>
        Ok(await proveedorService.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> Actualizar(int id, GuardarProveedorRequest request, CancellationToken ct) =>
        Ok(await proveedorService.ActualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await proveedorService.EliminarAsync(id, ct);
        return NoContent();
    }
}
