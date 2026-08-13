using GestaoColaboradores.API.Services.Colaboradores;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.API.Controllers;

[ApiController]
[Route("api/colaboradores")]
public class ColaboradoresController(IColaboradorService colaboradorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ColaboradorListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await colaboradorService.ListAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ColaboradorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateColaboradorRequest request, CancellationToken ct)
    {
        var result = await colaboradorService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ColaboradorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateColaboradorRequest request, CancellationToken ct)
    {
        var result = await colaboradorService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await colaboradorService.DeleteAsync(id, ct);
        return NoContent();
    }

}
