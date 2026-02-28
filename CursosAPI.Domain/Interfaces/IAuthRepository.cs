using CursosAPI.Domain.Entities;

namespace CursosAPI.Domain.Interfaces;

public interface IAuthRepository
{
    Task<Usuario?> AutenticarAsync(string email, string password);
}
