using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Services.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.API.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    {
        var result = await usuarioService.ListAsync(status, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request, CancellationToken ct)
    {
        var result = await usuarioService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request, CancellationToken ct)
    {
        var result = await usuarioService.UpdateAsync(id, request, ct);
        return Ok(result);
    }
}