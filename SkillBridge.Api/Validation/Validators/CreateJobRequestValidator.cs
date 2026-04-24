using FluentValidation;
using SkillBridge.Api.Contracts.Jobs;

namespace SkillBridge.Api.Validation.Validators;

public sealed class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.EmployerName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.EmployerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Location)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));
    }
}
