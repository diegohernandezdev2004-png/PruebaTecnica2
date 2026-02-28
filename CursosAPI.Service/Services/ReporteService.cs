using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Service.Interfaces;

namespace CursosAPI.Service.Services;

public class ReporteService : IReporteService
{
    private readonly IReporteRepository _repository;

    public ReporteService(IReporteRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<ReporteCategoriaDTO>> ReporteCursosPorCategoriaAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        return await _repository.ReporteCursosPorCategoriaAsync(page, pageSize, sortBy, sortDir, filtro);
    }

    public async Task<PaginatedResponse<ReporteInscritosDTO>> ReporteCursosMasInscritosAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        return await _repository.ReporteCursosMasInscritosAsync(page, pageSize, sortBy, sortDir, filtro);
    }
}
