using System.Data;
using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CursosAPI.Infrastructure.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly SqlDbContext _context;

    public ReporteRepository(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<ReporteCategoriaDTO>> ReporteCursosPorCategoriaAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Reportes_CursosPorCategoria", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@PageNumber", page);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrEmpty(sortBy) ? "Categoria" : sortBy);
        cmd.Parameters.AddWithValue("@SortDir", string.IsNullOrEmpty(sortDir) ? "asc" : sortDir);
        cmd.Parameters.AddWithValue("@Filtro", string.IsNullOrEmpty(filtro) ? DBNull.Value : (object)filtro);

        var reporte = new List<ReporteCategoriaDTO>();
        int totalCount = 0;
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reporte.Add(new ReporteCategoriaDTO
            {
                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                TotalCursos = reader.GetInt32(reader.GetOrdinal("TotalCursos")),
                PrecioPromedio = reader.GetDecimal(reader.GetOrdinal("PrecioPromedio"))
            });
        }
        
        // El usuario indicó que usemos NextResult() para obtener el TotalCount
        if (await reader.NextResultAsync())
        {
            if (await reader.ReadAsync())
            {
                // Supone que el segundo set de resultados trae la cuenta en la primera columna
                totalCount = reader.GetInt32(0);
            }
        }
        
        return new PaginatedResponse<ReporteCategoriaDTO>
        {
            Items = reporte,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResponse<ReporteInscritosDTO>> ReporteCursosMasInscritosAsync(int page, int pageSize, string sortBy, string sortDir, string filtro)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Reportes_CursosMasInscritos", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@PageNumber", page);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SortBy", string.IsNullOrEmpty(sortBy) ? "TotalInscritos" : sortBy);
        cmd.Parameters.AddWithValue("@SortDir", string.IsNullOrEmpty(sortDir) ? "desc" : sortDir);
        cmd.Parameters.AddWithValue("@Filtro", string.IsNullOrEmpty(filtro) ? DBNull.Value : (object)filtro);

        var reporte = new List<ReporteInscritosDTO>();
        int totalCount = 0;
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reporte.Add(new ReporteInscritosDTO
            {
                CursoId = reader.GetInt32(reader.GetOrdinal("CursoId")),
                NombreCurso = reader.GetString(reader.GetOrdinal("NombreCurso")),
                TotalInscritos = reader.GetInt32(reader.GetOrdinal("TotalInscritos"))
            });
        }
        
        // Obtener el TotalCount del siguiente conjunto de resultados
        if (await reader.NextResultAsync())
        {
            if (await reader.ReadAsync())
            {
                // Primera columna del segundo conjunto
                totalCount = reader.GetInt32(0);
            }
        }

        return new PaginatedResponse<ReporteInscritosDTO>
        {
            Items = reporte,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
