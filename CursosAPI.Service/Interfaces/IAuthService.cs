namespace CursosAPI.Service.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(string email, string password);
}
