using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize] // ambos roles pueden leer (catálogo/filtros); solo Administrador escribe
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken ct) =>
        Ok(await categoriaService.ListarAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<CategoriaDto>> Crear(GuardarCategoriaRequest request, CancellationToken ct) =>
        Ok(await categoriaService.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<CategoriaDto>> Actualizar(int id, GuardarCategoriaRequest request, CancellationToken ct) =>
        Ok(await categoriaService.ActualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await categoriaService.EliminarAsync(id, ct);
        return NoContent();
    }
}
