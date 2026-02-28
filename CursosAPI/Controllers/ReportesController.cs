using CursosAPI.Domain.DTOs;
using CursosAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,User")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    [HttpGet("cursos-por-categoria")]
    public async Task<IActionResult> GetCursosPorCategoria([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "Categoria", [FromQuery] string sortDir = "asc", [FromQuery] string filtro = "")
    {
        var result = await _reporteService.ReporteCursosPorCategoriaAsync(page, pageSize, sortBy, sortDir, filtro);
        return Ok(ApiResponse<PaginatedResponse<ReporteCategoriaDTO>>.Ok(result));
    }

    [HttpGet("cursos-mas-inscritos")]
    public async Task<IActionResult> GetCursosMasInscritos([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "TotalInscritos", [FromQuery] string sortDir = "desc", [FromQuery] string filtro = "")
    {
        var result = await _reporteService.ReporteCursosMasInscritosAsync(page, pageSize, sortBy, sortDir, filtro);
        return Ok(ApiResponse<PaginatedResponse<ReporteInscritosDTO>>.Ok(result));
    }
}
