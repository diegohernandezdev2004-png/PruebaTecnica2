namespace CursosAPI.Domain.DTOs;

public class ReporteInscritosDTO
{
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int TotalInscritos { get; set; }
}
