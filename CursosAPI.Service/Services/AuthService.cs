using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CursosAPI.Service.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository repository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        _logger.LogInformation("Intentando autenticar al usuario: {Email}", email);
        
        var usuario = await _repository.AutenticarAsync(email, password);
        if (usuario == null)
        {
            _logger.LogWarning("Error al autenticar usuario: {Email}. Credenciales incorrectas.", email);
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Clave JWT no configurada."));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        _logger.LogInformation("Usuario autenticado correctamente: {Email}", email);
        
        return tokenHandler.WriteToken(token);
    }
}
