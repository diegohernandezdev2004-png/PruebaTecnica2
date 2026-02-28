using CursosAPI.Domain.DTOs;

namespace CursosAPI.Domain.Interfaces;

public interface IReporteRepository
{
    Task<PaginatedResponse<ReporteCategoriaDTO>> ReporteCursosPorCategoriaAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
    Task<PaginatedResponse<ReporteInscritosDTO>> ReporteCursosMasInscritosAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
}
