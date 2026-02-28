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

public class CursoServiceTests
{
    private readonly Mock<ICursoRepository> _cursoRepoMock;
    private readonly Mock<IInstructorRepository> _instructorRepoMock;
    private readonly IValidator<CursoDTO> _validator;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CursoService>> _loggerMock;
    private readonly CursoService _service;

    public CursoServiceTests()
    {
        _cursoRepoMock = new Mock<ICursoRepository>();
        _instructorRepoMock = new Mock<IInstructorRepository>();
        _loggerMock = new Mock<ILogger<CursoService>>();
        _validator = new CursoValidator();

        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<IMemoryCache>();

        _service = new CursoService(_cursoRepoMock.Object, _instructorRepoMock.Object, _validator, _cache, _loggerMock.Object);
    }

    [Fact]
    public async Task CrearAsync_DebeFahar_SiPrecioSupera500()
    {
        var cursoDto = new CursoDTO
        {
            Nombre = "Curso Test",
            Categoria = "Programación",
            PrecioBase = 501m, // Supera los 500
            DuracionHoras = 20,
            InstructorId = 1,
            Activo = true
        };

        var result = await _service.CrearAsync(cursoDto);

        Assert.False(result.Success);
        Assert.Contains("El precio base no puede superar los $500", result.Message);
    }

    [Fact]
    public async Task CrearAsync_DebeFahar_SiDuracionInvalida()
    {
        var cursoDto = new CursoDTO
        {
            Nombre = "Curso Test",
            Categoria = "Programación",
            PrecioBase = 100m,
            DuracionHoras = 5, // Inválido (debe ser 10-200)
            InstructorId = 1,
            Activo = true
        };

        var result = await _service.CrearAsync(cursoDto);

        Assert.False(result.Success);
        Assert.Contains("La duración debe estar entre 10 y 200 horas", result.Message);
    }

    [Fact]
    public async Task CrearAsync_DebeFahar_SiInstructorSupera10Cursos()
    {
        var cursoDto = new CursoDTO
        {
            Nombre = "Curso Valido",
            Categoria = "Programación",
            PrecioBase = 100m,
            DuracionHoras = 20,
            InstructorId = 1,
            Activo = true
        };

        _cursoRepoMock.Setup(repo => repo.ExisteNombreCategoriaAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(false);

        _instructorRepoMock.Setup(repo => repo.ObtenerPorIdAsync(1))
                           .ReturnsAsync(new Instructor { Id = 1, Nombre = "Juan", Activo = true });

        _instructorRepoMock.Setup(repo => repo.ContarCursosActivosAsync(1))
                           .ReturnsAsync(10);

        var result = await _service.CrearAsync(cursoDto);

        Assert.False(result.Success);
        Assert.Contains("El instructor ya ha alcanzado el límite de 10 cursos activos", result.Message);
    }

    [Fact]
    public async Task CrearAsync_DebeSerExitoso_ConDatosValidos()
    {
        var cursoDto = new CursoDTO
        {
            Nombre = "Curso Valido",
            Categoria = "Programación",
            PrecioBase = 100m,
            DuracionHoras = 20,
            InstructorId = 1,
            Activo = true
        };

        _cursoRepoMock.Setup(repo => repo.ExisteNombreCategoriaAsync(cursoDto.Nombre, cursoDto.Categoria, null))
                      .ReturnsAsync(false);

        _instructorRepoMock.Setup(repo => repo.ObtenerPorIdAsync(cursoDto.InstructorId))
                           .ReturnsAsync(new Instructor { Id = 1, Nombre = "Juan", Activo = true });

        _instructorRepoMock.Setup(repo => repo.ContarCursosActivosAsync(cursoDto.InstructorId))
                           .ReturnsAsync(5);

        _cursoRepoMock.Setup(repo => repo.CrearAsync(It.IsAny<Curso>()))
                      .ReturnsAsync(1);

        var result = await _service.CrearAsync(cursoDto);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data);
    }

    [Fact]
    public async Task ListarAsync_DebeRetornarPaginatedResponse()
    {
        var r = new PaginatedResponse<Curso>
        {
            Items = new List<Curso> { new Curso { Id = 1, Nombre = "C1", Categoria = "Cat1", PrecioBase = 10m, DuracionHoras=10, InstructorId=1, Activo = true } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _cursoRepoMock.Setup(repo => repo.ListarAsync(1, 10, "Id", "asc", "")).ReturnsAsync(r);

        var result = await _service.ListarAsync(1, 10, "Id", "asc", "");

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("C1", result.Items.First().Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_RetornaCurso_SiExiste()
    {
        _cursoRepoMock.Setup(repo => repo.ObtenerPorIdAsync(1)).ReturnsAsync(new Curso { Id = 1, Nombre = "C1" });

        var result = await _service.ObtenerPorIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("C1", result.Nombre);
    }

    [Fact]
    public async Task ActualizarAsync_DebeSerExitoso_ConDatosValidos()
    {
        var cursoDto = new CursoDTO
        {
            Nombre = "Curso Valido",
            Categoria = "Programación",
            PrecioBase = 100m,
            DuracionHoras = 20,
            InstructorId = 1,
            Activo = true
        };

        _cursoRepoMock.Setup(repo => repo.ObtenerPorIdAsync(1))
            .ReturnsAsync(new Curso { Id = 1, Nombre = "C1", InstructorId = 1 });

        _cursoRepoMock.Setup(repo => repo.ExisteNombreCategoriaAsync(cursoDto.Nombre, cursoDto.Categoria, 1))
                      .ReturnsAsync(false);

        _instructorRepoMock.Setup(repo => repo.ObtenerPorIdAsync(cursoDto.InstructorId))
                           .ReturnsAsync(new Instructor { Id = 1, Nombre = "Juan", Activo = true });

        _instructorRepoMock.Setup(repo => repo.ContarCursosActivosAsync(cursoDto.InstructorId))
                           .ReturnsAsync(5);

        _cursoRepoMock.Setup(repo => repo.ActualizarAsync(It.IsAny<Curso>()))
                      .Returns(Task.CompletedTask);

        var result = await _service.ActualizarAsync(1, cursoDto);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task EliminarAsync_DebeSerExitoso()
    {
        _cursoRepoMock.Setup(repo => repo.ObtenerPorIdAsync(1)).ReturnsAsync(new Curso { Id = 1, Nombre = "C1" });
        _cursoRepoMock.Setup(repo => repo.EliminarAsync(1)).Returns(Task.CompletedTask);

        var result = await _service.EliminarAsync(1);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }
}
