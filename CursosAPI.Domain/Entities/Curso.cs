namespace CursosAPI.Domain.Entities;

public class Curso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public int DuracionHoras { get; set; }
    public int InstructorId { get; set; }
    public bool Activo { get; set; }
}
