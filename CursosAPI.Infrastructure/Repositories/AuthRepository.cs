using System.Data;
using CursosAPI.Domain.Entities;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CursosAPI.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly SqlDbContext _context;

    public AuthRepository(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string password)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_Usuarios_Autenticar", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@Password", password);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Rol = reader.GetString(reader.GetOrdinal("Rol"))
            };
        }
        return null;
    }
}
