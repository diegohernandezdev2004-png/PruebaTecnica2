using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Service.Interfaces;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CursosAPI.Service.Services;

public class CursoService : ICursoService
{
    private readonly ICursoRepository _cursoRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IValidator<CursoDTO> _validator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CursoService> _logger;

    public CursoService(ICursoRepository cursoRepository, IInstructorRepository instructorRepository, IValidator<CursoDTO> validator, IMemoryCache cache, ILogger<CursoService> logger)
    {
        _cursoRepository = cursoRepository;
        _instructorRepository = instructorRepository;
        _validator = validator;
        _cache = cache;
        _logger = logger;
    }

    private string ObtenerPrefijoCache()
    {
        return _cache.GetOrCreate("CursosCacheVersion", entry => Guid.NewGuid().ToString()) ?? "";
    }

    private void InvalidarCache()
    {
        _cache.Set("CursosCacheVersion", Guid.NewGuid().ToString());
    }

    public async Task<PaginatedResponse<CursoDTO>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        var cacheKey = $"{ObtenerPrefijoCache()}_cursos_{page}_{pageSize}_{sortBy}_{sortDir}_{filtro}";
        if (!_cache.TryGetValue(cacheKey, out PaginatedResponse<CursoDTO>? result))
        {
            var res = await _cursoRepository.ListarAsync(page, pageSize, sortBy, sortDir, filtro);
            
            // Map Entidad a DTO
            var dtoList = res.Items.Select(c => new CursoDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Categoria = c.Categoria,
                PrecioBase = c.PrecioBase,
                DuracionHoras = c.DuracionHoras,
                InstructorId = c.InstructorId,
                Activo = c.Activo
            }).ToList();

            result = new PaginatedResponse<CursoDTO>
            {
                Items = dtoList,
                TotalCount = res.TotalCount,
                Page = res.Page,
                PageSize = res.PageSize
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }
        return result!;
    }

    public async Task<CursoDTO?> ObtenerPorIdAsync(int id)
    {
        var c = await _cursoRepository.ObtenerPorIdAsync(id);
        if (c == null) return null;
        
        return new CursoDTO
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Categoria = c.Categoria,
            PrecioBase = c.PrecioBase,
            DuracionHoras = c.DuracionHoras,
            InstructorId = c.InstructorId,
            Activo = c.Activo
        };
    }

    public async Task<ApiResponse<int>> CrearAsync(CursoDTO cursoDto)
    {
        try
        {
            _logger.LogInformation("Creando nuevo curso: {Nombre}", cursoDto.Nombre);
            
            var validationResult = await _validator.ValidateAsync(cursoDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validación fallida para nuevo curso: {Nombre}", cursoDto.Nombre);
                return ApiResponse<int>.Error(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var existeClaveUnica = await _cursoRepository.ExisteNombreCategoriaAsync(cursoDto.Nombre, cursoDto.Categoria);
            if (existeClaveUnica)
                return ApiResponse<int>.Error("Ya existe un curso con el mismo nombre y categoría.");

            var instructor = await _instructorRepository.ObtenerPorIdAsync(cursoDto.InstructorId);
            if (instructor == null || !instructor.Activo)
                return ApiResponse<int>.Error("El instructor asignado no existe o no está activo.");

            var cursosInstructor = await _instructorRepository.ContarCursosActivosAsync(cursoDto.InstructorId);
            if (cursosInstructor >= 10)
                return ApiResponse<int>.Error("El instructor ya ha alcanzado el límite de 10 cursos activos.");

            var cursoEntity = new Curso
            {
                Nombre = cursoDto.Nombre,
                Categoria = cursoDto.Categoria,
                PrecioBase = cursoDto.PrecioBase,
                DuracionHoras = cursoDto.DuracionHoras,
                InstructorId = cursoDto.InstructorId,
                Activo = cursoDto.Activo
            };

            var id = await _cursoRepository.CrearAsync(cursoEntity);
            InvalidarCache(); // Limpiar cache
            return ApiResponse<int>.Ok(id);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear el curso {Nombre}.", cursoDto.Nombre);
            return ApiResponse<int>.Error($"Error de BD: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ActualizarAsync(int id, CursoDTO cursoDto)
    {
        try
        {
            _logger.LogInformation("Actualizando curso ID: {Id}", id);
            
            cursoDto.Id = id;
            var validationResult = await _validator.ValidateAsync(cursoDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validación fallida para modificar curso: {Id}", id);
                return ApiResponse<bool>.Error(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var existeClaveUnica = await _cursoRepository.ExisteNombreCategoriaAsync(cursoDto.Nombre, cursoDto.Categoria, id);
            if (existeClaveUnica)
                return ApiResponse<bool>.Error("Ya existe otro curso con el mismo nombre y categoría.");

            var instructor = await _instructorRepository.ObtenerPorIdAsync(cursoDto.InstructorId);
            if (instructor == null || !instructor.Activo)
                return ApiResponse<bool>.Error("El instructor asignado no existe o no está activo.");

            var cursoActual = await _cursoRepository.ObtenerPorIdAsync(id);
            if (cursoActual == null) return ApiResponse<bool>.Error("Curso no encontrado.");

            if (cursoDto.InstructorId != cursoActual.InstructorId || (!cursoActual.Activo && cursoDto.Activo))
            {
                var cursosInstructor = await _instructorRepository.ContarCursosActivosAsync(cursoDto.InstructorId);
                if (cursosInstructor >= 10)
                    return ApiResponse<bool>.Error("El instructor ya ha alcanzado el límite de 10 cursos activos.");
            }

            var cursoEntity = new Curso
            {
                Id = id,
                Nombre = cursoDto.Nombre,
                Categoria = cursoDto.Categoria,
                PrecioBase = cursoDto.PrecioBase,
                DuracionHoras = cursoDto.DuracionHoras,
                InstructorId = cursoDto.InstructorId,
                Activo = cursoDto.Activo
            };

            await _cursoRepository.ActualizarAsync(cursoEntity);
            InvalidarCache();
            return ApiResponse<bool>.Ok(true);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos al modificar el curso {Id}.", id);
            return ApiResponse<bool>.Error($"Error de BD: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarAsync(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando curso ID: {Id}", id);
            
            var curso = await _cursoRepository.ObtenerPorIdAsync(id);
            if (curso == null) return ApiResponse<bool>.Error("Curso no encontrado.");

            await _cursoRepository.EliminarAsync(id);
            InvalidarCache();
            return ApiResponse<bool>.Ok(true);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos al eliminar el curso {Id}.", id);
            return ApiResponse<bool>.Error($"Error de BD: {ex.Message}");
        }
    }
}
