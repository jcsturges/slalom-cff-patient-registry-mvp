using FluentValidation;
using NgrApi.DTOs;

namespace NgrApi.Validators;

public class CreateCareProgramDtoValidator : AbstractValidator<CreateCareProgramDto>
{
    public CreateCareProgramDtoValidator()
    {
        RuleFor(x => x.ProgramId)
            .GreaterThan(0).WithMessage("Program ID must be a positive integer");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Program name is required")
            .MaximumLength(200).WithMessage("Program name must not exceed 200 characters");

        RuleFor(x => x.ProgramType)
            .NotEmpty().WithMessage("Program type is required")
            .Must(t => ProgramConstants.ValidProgramTypes.Contains(t))
            .WithMessage("Program type must be one of: Pediatric, Adult, Affiliate, Orphaned-Record Holding, Training");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null);

        RuleFor(x => x.State)
            .MaximumLength(2).When(x => x.State != null);

        RuleFor(x => x.ZipCode)
            .MaximumLength(10).When(x => x.ZipCode != null);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email address format");
    }
}

public class UpdateCareProgramDtoValidator : AbstractValidator<UpdateCareProgramDto>
{
    public UpdateCareProgramDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Program name is required")
            .MaximumLength(200).WithMessage("Program name must not exceed 200 characters");

        RuleFor(x => x.ProgramType)
            .NotEmpty().WithMessage("Program type is required")
            .Must(t => ProgramConstants.ValidProgramTypes.Contains(t))
            .WithMessage("Program type must be one of: Pediatric, Adult, Affiliate, Orphaned-Record Holding, Training");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null);

        RuleFor(x => x.State)
            .MaximumLength(2).When(x => x.State != null);

        RuleFor(x => x.ZipCode)
            .MaximumLength(10).When(x => x.ZipCode != null);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email address format");
    }
}
