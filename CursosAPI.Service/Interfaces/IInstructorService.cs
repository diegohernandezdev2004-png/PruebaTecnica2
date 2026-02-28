using CursosAPI.Domain.DTOs;

namespace CursosAPI.Service.Interfaces;

public interface IInstructorService
{
    Task<PaginatedResponse<InstructorDTO>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
    Task<InstructorDTO?> ObtenerPorIdAsync(int id);
    Task<ApiResponse<int>> CrearAsync(InstructorDTO instructorDto);
    Task<ApiResponse<bool>> ActualizarAsync(int id, InstructorDTO instructorDto);
    Task<ApiResponse<bool>> EliminarAsync(int id);
}
