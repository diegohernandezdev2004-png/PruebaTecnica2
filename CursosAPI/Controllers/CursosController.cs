using CursosAPI.Domain.DTOs;
using CursosAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class CursosController : ControllerBase
{
    private readonly ICursoService _cursoService;

    public CursosController(ICursoService cursoService)
    {
        _cursoService = cursoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "Id", [FromQuery] string sortDir = "asc", [FromQuery] string filtro = "")
    {
        var result = await _cursoService.ListarAsync(page, pageSize, sortBy, sortDir, filtro);
        return Ok(ApiResponse<PaginatedResponse<CursoDTO>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var curso = await _cursoService.ObtenerPorIdAsync(id);
        if (curso == null) return NotFound(ApiResponse<string>.Error("Curso no encontrado."));
        return Ok(ApiResponse<CursoDTO>.Ok(curso));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] CursoDTO curso)
    {
        var result = await _cursoService.CrearAsync(curso);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put(int id, [FromBody] CursoDTO curso)
    {
        var result = await _cursoService.ActualizarAsync(id, curso);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _cursoService.EliminarAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
