using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Service.Interfaces;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CursosAPI.Service.Services;

public class InstructorService : IInstructorService
{
    private readonly IInstructorRepository _repository;
    private readonly IValidator<InstructorDTO> _validator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InstructorService> _logger;

    public InstructorService(IInstructorRepository repository, IValidator<InstructorDTO> validator, IMemoryCache cache, ILogger<InstructorService> logger)
    {
        _repository = repository;
        _validator = validator;
        _cache = cache;
        _logger = logger;
    }

    private string ObtenerPrefijoCache()
    {
        return _cache.GetOrCreate("InstructoresCacheVersion", entry => Guid.NewGuid().ToString()) ?? "";
    }

    private void InvalidarCache()
    {
        _cache.Set("InstructoresCacheVersion", Guid.NewGuid().ToString());
    }

    public async Task<PaginatedResponse<InstructorDTO>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        var cacheKey = $"{ObtenerPrefijoCache()}_instructores_{page}_{pageSize}_{sortBy}_{sortDir}_{filtro}";
        if (!_cache.TryGetValue(cacheKey, out PaginatedResponse<InstructorDTO>? result))
        {
            var res = await _repository.ListarAsync(page, pageSize, sortBy, sortDir, filtro);

            // Map Entidad a DTO
            var dtoList = res.Items.Select(i => new InstructorDTO
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Apellidos = i.Apellidos,
                Email = i.Email,
                Activo = i.Activo
            }).ToList();

            result = new PaginatedResponse<InstructorDTO>
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

    public async Task<InstructorDTO?> ObtenerPorIdAsync(int id)
    {
        var i = await _repository.ObtenerPorIdAsync(id);
        if (i == null) return null;

        return new InstructorDTO
        {
            Id = i.Id,
            Nombre = i.Nombre,
            Apellidos = i.Apellidos,
            Email = i.Email,
            Activo = i.Activo
        };
    }

    public async Task<ApiResponse<int>> CrearAsync(InstructorDTO instructorDto)
    {
        try
        {
            _logger.LogInformation("Creando nuevo instructor: {Email}", instructorDto.Email);
            
            var validationResult = await _validator.ValidateAsync(instructorDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validación fallida para nuevo instructor: {Email}", instructorDto.Email);
                return ApiResponse<int>.Error(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var entity = new Instructor
            {
                Nombre = instructorDto.Nombre,
                Apellidos = instructorDto.Apellidos,
                Email = instructorDto.Email,
                Activo = instructorDto.Activo
            };

            var id = await _repository.CrearAsync(entity);
            InvalidarCache();
            return ApiResponse<int>.Ok(id);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear el instructor {Email}.", instructorDto.Email);
            return ApiResponse<int>.Error($"Error de BD: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ActualizarAsync(int id, InstructorDTO instructorDto)
    {
        try
        {
            _logger.LogInformation("Actualizando instructor ID: {Id}", id);
            
            instructorDto.Id = id;
            var validationResult = await _validator.ValidateAsync(instructorDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validación fallida para modificar instructor: {Id}", id);
                return ApiResponse<bool>.Error(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var entity = new Instructor
            {
                Id = id,
                Nombre = instructorDto.Nombre,
                Apellidos = instructorDto.Apellidos,
                Email = instructorDto.Email,
                Activo = instructorDto.Activo
            };

            await _repository.ActualizarAsync(entity);
            InvalidarCache();
            return ApiResponse<bool>.Ok(true);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos al modificar el instructor {Id}.", id);
            return ApiResponse<bool>.Error($"Error de BD: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarAsync(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando instructor ID: {Id}", id);
            
            await _repository.EliminarAsync(id);
            InvalidarCache();
            return ApiResponse<bool>.Ok(true);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos al eliminar el instructor {Id}.", id);
            return ApiResponse<bool>.Error($"Error de BD: {ex.Message}");
        }
    }
}
