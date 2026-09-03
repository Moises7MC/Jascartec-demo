using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize] // ambos roles pueden ver el catálogo; solo Administrador lo edita
public class ProductosController(IProductoService productoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Listar(CancellationToken ct) =>
        Ok(await productoService.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await productoService.ObtenerAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProductoDto>> Crear(GuardarProductoRequest request, CancellationToken ct) =>
        Ok(await productoService.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, GuardarProductoRequest request, CancellationToken ct) =>
        Ok(await productoService.ActualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await productoService.EliminarAsync(id, ct);
        return NoContent();
    }
}
