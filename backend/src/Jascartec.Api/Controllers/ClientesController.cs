using Jascartec.Application.Dtos;
using Jascartec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize] // Administrador y Vendedor gestionan clientes por igual
public class ClientesController(IClienteService clienteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> Listar(CancellationToken ct) =>
        Ok(await clienteService.ListarAsync(ct));

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(GuardarClienteRequest request, CancellationToken ct) =>
        Ok(await clienteService.CrearAsync(request, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, GuardarClienteRequest request, CancellationToken ct) =>
        Ok(await clienteService.ActualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await clienteService.EliminarAsync(id, ct);
        return NoContent();
    }
}
