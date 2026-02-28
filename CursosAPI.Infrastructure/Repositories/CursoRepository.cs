using System.Data;
using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CursosAPI.Infrastructure.Repositories;

public class CursoRepository : ICursoRepository
{
    private readonly SqlDbContext _context;

    public CursoRepository(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<int> CrearAsync(Curso curso)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Cursos_Crear", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Nombre", curso.Nombre);
        cmd.Parameters.AddWithValue("@Categoria", curso.Categoria);
        cmd.Parameters.AddWithValue("@PrecioBase", curso.PrecioBase);
        cmd.Parameters.AddWithValue("@DuracionHoras", curso.DuracionHoras);
        cmd.Parameters.AddWithValue("@InstructorId", curso.InstructorId);
        cmd.Parameters.AddWithValue("@Activo", curso.Activo);

        // Si el stored procedure usa SELECT SCOPE_IDENTITY, ExecuteScalar obtiene el Id
        var idParam = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(idParam);

        await cmd.ExecuteNonQueryAsync();

        if (idParam.Value != DBNull.Value && idParam.Value != null)
            return Convert.ToInt32(idParam.Value);
        
        return 0;
    }

    public async Task ActualizarAsync(Curso curso)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Cursos_Actualizar", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Id", curso.Id);
        cmd.Parameters.AddWithValue("@Nombre", curso.Nombre);
        cmd.Parameters.AddWithValue("@Categoria", curso.Categoria);
        cmd.Parameters.AddWithValue("@PrecioBase", curso.PrecioBase);
        cmd.Parameters.AddWithValue("@DuracionHoras", curso.DuracionHoras);
        cmd.Parameters.AddWithValue("@InstructorId", curso.InstructorId);
        cmd.Parameters.AddWithValue("@Activo", curso.Activo);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task EliminarAsync(int id)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Cursos_Eliminar", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExisteNombreCategoriaAsync(string nombre, string categoria, int? excludeId = null)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Cursos_ExisteNombreCategoria", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Nombre", nombre);
        cmd.Parameters.AddWithValue("@Categoria", categoria);
        cmd.Parameters.AddWithValue("@ExcludeId", excludeId ?? (object)DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<PaginatedResponse<Curso>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Cursos_Listar", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@PageNumber", page);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SortBy", sortBy ?? "Id");
        cmd.Parameters.AddWithValue("@SortDir", sortDir ?? "asc");
        cmd.Parameters.AddWithValue("@Filtro", string.IsNullOrEmpty(filtro) ? DBNull.Value : (object)filtro);

        var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(totalCountParam);

        var cursos = new List<Curso>();
        using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            cursos.Add(new Curso
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                PrecioBase = reader.GetDecimal(reader.GetOrdinal("PrecioBase")),
                DuracionHoras = reader.GetInt32(reader.GetOrdinal("DuracionHoras")),
                InstructorId = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
            });
        }
        
        await reader.CloseAsync();

        return new PaginatedResponse<Curso>
        {
            Items = cursos,
            TotalCount = totalCountParam.Value != DBNull.Value ? (int)totalCountParam.Value : 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Curso?> ObtenerPorIdAsync(int id)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Cursos_ObtenerPorId", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Curso
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                PrecioBase = reader.GetDecimal(reader.GetOrdinal("PrecioBase")),
                DuracionHoras = reader.GetInt32(reader.GetOrdinal("DuracionHoras")),
                InstructorId = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
        return null;
    }
}
