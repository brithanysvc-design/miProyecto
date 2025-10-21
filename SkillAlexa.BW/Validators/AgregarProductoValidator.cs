using FluentValidation;
using SkillAlexa.BW.DTOs;

namespace SkillAlexa.BW.Validators;

public class AgregarProductoValidator: AbstractValidator<AgregarProductoDto>
{
    public AgregarProductoValidator()
    {
        RuleFor(x => x.IdLista)
            .NotEmpty().WithMessage("El ID de la lista es requerido");

        RuleFor(x => x.NombreProducto)
            .NotEmpty().WithMessage("El nombre del producto es requerido")
            .MaximumLength(200).WithMessage("El nombre del producto no puede exceder 200 caracteres")
            .MinimumLength(2).WithMessage("El nombre del producto debe tener al menos 2 caracteres");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero")
            .LessThanOrEqualTo(9999).WithMessage("La cantidad no puede exceder 9999");

        RuleFor(x => x.Unidad)
            .MaximumLength(50).WithMessage("La unidad no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Unidad));
    }
}