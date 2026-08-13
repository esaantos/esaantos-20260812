using GestaoColaboradores.API.Services.Unidades;
using GestaoColaboradores.API.Services.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.API.Controllers;

[ApiController]
[Route("api/unidades")]
public class UnidadesController(IUnidadeService unidadeService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<UnidadeListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await unidadeService.ListAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnidadeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUnidadeRequest request, CancellationToken ct)
    {
        var result = await unidadeService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UnidadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnidadeStatusRequest request, CancellationToken ct)
    {
        var result = await unidadeService.UpdateAsync(id, request, ct);
        return Ok(result);
    }
}
