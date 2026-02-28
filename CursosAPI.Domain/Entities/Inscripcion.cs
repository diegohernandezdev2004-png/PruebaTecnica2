namespace CursosAPI.Domain.Entities;

public class Inscripcion
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaInscripcion { get; set; }
}
