using FluentValidation;
using SkillBridge.Api.Contracts.Users;

namespace SkillBridge.Api.Validation.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    private static readonly string[] AllowedRoles = ["youth", "mentor", "admin", "employer"];

    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Location)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.Bio)
            .MaximumLength(1200)
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.Education)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Education));

        RuleFor(x => x.CareerGoal)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.CareerGoal));

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AllowedRoles.Contains(role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be one of: youth, mentor, admin, employer.");
    }
}
