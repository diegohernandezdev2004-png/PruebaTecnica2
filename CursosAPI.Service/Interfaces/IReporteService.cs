using CursosAPI.Domain.DTOs;

namespace CursosAPI.Service.Interfaces;

public interface IReporteService
{
    Task<PaginatedResponse<ReporteCategoriaDTO>> ReporteCursosPorCategoriaAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
    Task<PaginatedResponse<ReporteInscritosDTO>> ReporteCursosMasInscritosAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
}
