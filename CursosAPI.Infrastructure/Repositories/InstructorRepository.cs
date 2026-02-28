using System.Data;
using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CursosAPI.Infrastructure.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly SqlDbContext _context;

    public InstructorRepository(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<int> ContarCursosActivosAsync(int instructorId)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Instructores_ContarCursosActivos", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@InstructorId", instructorId);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> CrearAsync(Instructor instructor)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Instructores_Crear", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Nombre", instructor.Nombre);
        cmd.Parameters.AddWithValue("@Apellidos", instructor.Apellidos);
        cmd.Parameters.AddWithValue("@Email", instructor.Email);
        cmd.Parameters.AddWithValue("@Activo", instructor.Activo);

        var idParam = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(idParam);

        await cmd.ExecuteNonQueryAsync();

        if (idParam.Value != DBNull.Value && idParam.Value != null)
            return Convert.ToInt32(idParam.Value);
        
        return 0;
    }

    public async Task ActualizarAsync(Instructor instructor)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Instructores_Actualizar", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Id", instructor.Id);
        cmd.Parameters.AddWithValue("@Nombre", instructor.Nombre);
        cmd.Parameters.AddWithValue("@Apellidos", instructor.Apellidos);
        cmd.Parameters.AddWithValue("@Email", instructor.Email);
        cmd.Parameters.AddWithValue("@Activo", instructor.Activo);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task EliminarAsync(int id)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Instructores_Eliminar", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<PaginatedResponse<Instructor>> ListarAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Instructores_Listar", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@PageNumber", page);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SortBy", sortBy ?? "Id");
        cmd.Parameters.AddWithValue("@SortDir", sortDir ?? "asc");
        cmd.Parameters.AddWithValue("@Filtro", string.IsNullOrEmpty(filtro) ? DBNull.Value : (object)filtro);

        var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(totalCountParam);

        var list = new List<Instructor>();
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Instructor
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellidos = reader.GetString(reader.GetOrdinal("Apellidos")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
            });
        }
        await reader.CloseAsync();

        return new PaginatedResponse<Instructor>
        {
            Items = list,
            TotalCount = totalCountParam.Value != DBNull.Value ? (int)totalCountParam.Value : 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Instructor?> ObtenerPorIdAsync(int id)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Instructores_ObtenerPorId", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Instructor
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellidos = reader.GetString(reader.GetOrdinal("Apellidos")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
        return null;
    }
}
