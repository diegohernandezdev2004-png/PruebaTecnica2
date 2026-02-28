using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;

namespace CursosAPI.Domain.Interfaces;

public interface ICursoRepository
{
    Task<PaginatedResponse<Curso>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro);
    Task<Curso?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Curso curso);
    Task ActualizarAsync(Curso curso);
    Task EliminarAsync(int id);
    Task<bool> ExisteNombreCategoriaAsync(string nombre, string categoria, int? excludeId = null);
    
}
