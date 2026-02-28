using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;

namespace CursosAPI.Domain.Interfaces;

public interface IInstructorRepository
{
    Task<PaginatedResponse<Instructor>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
    Task<Instructor?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Instructor instructor);
    Task ActualizarAsync(Instructor instructor);
    Task EliminarAsync(int id);
    Task<int> ContarCursosActivosAsync(int instructorId);
}
