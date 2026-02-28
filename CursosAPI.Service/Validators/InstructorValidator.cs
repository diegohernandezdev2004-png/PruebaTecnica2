using CursosAPI.Domain.DTOs;
using FluentValidation;

namespace CursosAPI.Service.Validators;

public class InstructorValidator : AbstractValidator<InstructorDTO>
{
    public InstructorValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(100);
        RuleFor(x => x.Apellidos).NotEmpty().WithMessage("Los apellidos son requeridos.").MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().WithMessage("El email es requerido.").EmailAddress().WithMessage("Formato de email inválido.");
    }
}
