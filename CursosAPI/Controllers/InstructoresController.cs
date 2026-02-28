using CursosAPI.Domain.DTOs;
using CursosAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class InstructoresController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructoresController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "Id", [FromQuery] string sortDir = "asc", [FromQuery] string filtro = "")
    {
        var result = await _instructorService.ListarAsync(page, pageSize, sortBy, sortDir, filtro);
        return Ok(ApiResponse<PaginatedResponse<InstructorDTO>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _instructorService.ObtenerPorIdAsync(id);
        if (instructor == null) return NotFound(ApiResponse<string>.Error("Instructor no encontrado."));
        return Ok(ApiResponse<InstructorDTO>.Ok(instructor));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] InstructorDTO instructor)
    {
        var result = await _instructorService.CrearAsync(instructor);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put(int id, [FromBody] InstructorDTO instructor)
    {
        var result = await _instructorService.ActualizarAsync(id, instructor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _instructorService.EliminarAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
