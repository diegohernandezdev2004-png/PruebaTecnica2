using CursosAPI.Domain.DTOs;
using FluentValidation;

namespace CursosAPI.Service.Validators;

public class CursoValidator : AbstractValidator<CursoDTO>
{
    public CursoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(200);
        RuleFor(x => x.Categoria).NotEmpty().WithMessage("La categoría es requerida.").MaximumLength(100);
        RuleFor(x => x.PrecioBase).LessThanOrEqualTo(500).WithMessage("El precio base no puede superar los $500.");
        RuleFor(x => x.DuracionHoras).InclusiveBetween(10, 200).WithMessage("La duración debe estar entre 10 y 200 horas.");
        RuleFor(x => x.InstructorId).GreaterThan(0).WithMessage("Debe asignar un instructor válido.");
    }
}
