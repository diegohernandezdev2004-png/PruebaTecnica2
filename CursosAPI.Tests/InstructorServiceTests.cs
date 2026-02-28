using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Entities;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Service.Services;
using CursosAPI.Service.Validators;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CursosAPI.Tests;

public class InstructorServiceTests
{
    private readonly Mock<IInstructorRepository> _repoMock;
    private readonly IValidator<InstructorDTO> _validator;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<InstructorService>> _loggerMock;
    private readonly InstructorService _service;

    public InstructorServiceTests()
    {
        _repoMock = new Mock<IInstructorRepository>();
        _loggerMock = new Mock<ILogger<InstructorService>>();
        _validator = new InstructorValidator();

        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<IMemoryCache>();

        _service = new InstructorService(_repoMock.Object, _validator, _cache, _loggerMock.Object);
    }

    [Fact]
    public async Task CrearAsync_DebeSerExitoso_ConDatosValidos()
    {
        var instructorDto = new InstructorDTO
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            Activo = true
        };

        _repoMock.Setup(repo => repo.CrearAsync(It.IsAny<Instructor>())).ReturnsAsync(1);

        var result = await _service.CrearAsync(instructorDto);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data);
    }

    [Fact]
    public async Task CrearAsync_DebeFallar_ConDatosInvalidos()
    {
        var instructorDto = new InstructorDTO
        {
            Nombre = "", // Vacío, debería fallar
            Apellidos = "Pérez",
            Email = "correo_no_valido",
            Activo = true
        };

        var result = await _service.CrearAsync(instructorDto);

        Assert.False(result.Success);
        Assert.Contains("El nombre es requerido", result.Message);
        Assert.Contains("Formato de email inválido", result.Message);
    }

    [Fact]
    public async Task ListarAsync_DebeRetornarPaginatedResponse()
    {
        var r = new PaginatedResponse<Instructor>
        {
            Items = new List<Instructor> { new Instructor { Id = 1, Nombre = "Alan", Apellidos = "B", Email = "a@b.com", Activo = true } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _repoMock.Setup(repo => repo.ListarAsync(1, 10, "Id", "asc", "")).ReturnsAsync(r);

        var result = await _service.ListarAsync(1, 10, "Id", "asc", "");

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alan", result.Items.First().Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_RetornaInstructor_SiExiste()
    {
        _repoMock.Setup(repo => repo.ObtenerPorIdAsync(1)).ReturnsAsync(new Instructor { Id = 1, Nombre = "Alan" });

        var result = await _service.ObtenerPorIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Alan", result.Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_RetornaNull_SiNoExiste()
    {
        _repoMock.Setup(repo => repo.ObtenerPorIdAsync(1)).ReturnsAsync((Instructor)null);

        var result = await _service.ObtenerPorIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task ActualizarAsync_DebeSerExitoso_ConDatosValidos()
    {
        var instructorDto = new InstructorDTO
        {
            Nombre = "Juan Edit",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            Activo = true
        };

        _repoMock.Setup(repo => repo.ActualizarAsync(It.IsAny<Instructor>())).Returns(Task.CompletedTask);

        var result = await _service.ActualizarAsync(1, instructorDto);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task EliminarAsync_DebeSerExitoso()
    {
        _repoMock.Setup(repo => repo.EliminarAsync(1)).Returns(Task.CompletedTask);

        var result = await _service.EliminarAsync(1);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }
}
