using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/marcas")]
[Authorize] // ambos roles pueden leer (filtro de catálogo); solo Administrador escribe
public class MarcasController(IMarcaService marcaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MarcaDto>>> Listar(CancellationToken ct) =>
        Ok(await marcaService.ListarAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<MarcaDto>> Crear(GuardarMarcaRequest request, CancellationToken ct) =>
        Ok(await marcaService.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<MarcaDto>> Actualizar(int id, GuardarMarcaRequest request, CancellationToken ct) =>
        Ok(await marcaService.ActualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await marcaService.EliminarAsync(id, ct);
        return NoContent();
    }
}
