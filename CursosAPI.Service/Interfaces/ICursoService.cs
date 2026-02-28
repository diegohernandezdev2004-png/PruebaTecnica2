using CursosAPI.Domain.DTOs;

namespace CursosAPI.Service.Interfaces;

public interface ICursoService
{
    Task<PaginatedResponse<CursoDTO>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
    Task<CursoDTO?> ObtenerPorIdAsync(int id);
    Task<ApiResponse<int>> CrearAsync(CursoDTO cursoDto);
    Task<ApiResponse<bool>> ActualizarAsync(int id, CursoDTO cursoDto);
    Task<ApiResponse<bool>> EliminarAsync(int id);
}
