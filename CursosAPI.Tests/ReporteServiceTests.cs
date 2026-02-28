using CursosAPI.Domain.DTOs;
using CursosAPI.Domain.Interfaces;
using CursosAPI.Service.Services;
using Moq;
using Xunit;

namespace CursosAPI.Tests;

public class ReporteServiceTests
{
    private readonly Mock<IReporteRepository> _repoMock;
    private readonly ReporteService _service;

    public ReporteServiceTests()
    {
        _repoMock = new Mock<IReporteRepository>();
        _service = new ReporteService(_repoMock.Object);
    }

    [Fact]
    public async Task ReporteCursosPorCategoriaAsync_ShouldReturnData()
    {
        var mockResponse = new PaginatedResponse<ReporteCategoriaDTO>
        {
            Items = new List<ReporteCategoriaDTO> { new ReporteCategoriaDTO { Categoria = "Tech", TotalCursos = 5 } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _repoMock.Setup(repo => repo.ReporteCursosPorCategoriaAsync(1, 10, "Categoria", "asc", ""))
            .ReturnsAsync(mockResponse);

        var result = await _service.ReporteCursosPorCategoriaAsync(1, 10, "Categoria", "asc", "");

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Tech", result.Items.First().Categoria);
    }

    [Fact]
    public async Task ReporteCursosMasInscritosAsync_ShouldReturnData()
    {
        var mockResponse = new PaginatedResponse<ReporteInscritosDTO>
        {
            Items = new List<ReporteInscritosDTO> { new ReporteInscritosDTO { CursoId = 1, NombreCurso = "C#", TotalInscritos = 100 } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _repoMock.Setup(repo => repo.ReporteCursosMasInscritosAsync(1, 10, "TotalInscritos", "desc", ""))
            .ReturnsAsync(mockResponse);

        var result = await _service.ReporteCursosMasInscritosAsync(1, 10, "TotalInscritos", "desc", "");

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("C#", result.Items.First().NombreCurso);
    }
}
