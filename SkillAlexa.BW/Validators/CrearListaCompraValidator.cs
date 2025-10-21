using FluentValidation;
using SkillAlexa.BW.DTOs;

namespace SkillAlexa.BW.Validators;

public class CrearListaCompraValidator: AbstractValidator<CrearListaCompraDto>
{
    public CrearListaCompraValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la lista es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres");

        RuleFor(x => x.FechaObjetivo)
            .NotEmpty().WithMessage("La fecha objetivo es requerida")
            .Must(fecha => fecha.Date >= DateTime.UtcNow.Date)
            .WithMessage("La fecha objetivo no puede ser anterior al día de hoy");
    }
}