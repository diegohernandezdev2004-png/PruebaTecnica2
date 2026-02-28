namespace CursosAPI.Domain.DTOs;

public class ReporteCategoriaDTO
{
    public string Categoria { get; set; } = string.Empty;
    public int TotalCursos { get; set; }
    public decimal PrecioPromedio { get; set; }
}
