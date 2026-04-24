using FluentValidation;
using FluentValidation.Results;

namespace SkillBridge.Api.Validation;

public static class ValidationExtensions
{
    public static RouteGroupBuilder WithRequestValidation(this RouteGroupBuilder group)
    {
        group.AddEndpointFilterFactory((factoryContext, next) =>
        {
            var validationTargets = factoryContext.MethodInfo
                .GetParameters()
                .Select((parameter, index) => new
                {
                    Index = index,
                    ValidatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType)
                })
                .Where(x => factoryContext.ApplicationServices.GetService(x.ValidatorType) is not null)
                .ToArray();

            if (validationTargets.Length == 0)
            {
                return next;
            }

            return async invocationContext =>
            {
                var failures = new List<ValidationFailure>();

                foreach (var target in validationTargets)
                {
                    var argument = invocationContext.Arguments[target.Index];
                    if (argument is null)
                    {
                        continue;
                    }

                    var validator = (IValidator)invocationContext.HttpContext.RequestServices
                        .GetRequiredService(target.ValidatorType);

                    var context = new ValidationContext<object>(argument);
                    var result = await validator.ValidateAsync(context, invocationContext.HttpContext.RequestAborted);

                    if (!result.IsValid)
                    {
                        failures.AddRange(result.Errors);
                    }
                }

                if (failures.Count > 0)
                {
                    var errors = failures
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group
                                .Select(x => x.ErrorMessage)
                                .Distinct()
                                .ToArray());

                    return Results.ValidationProblem(errors);
                }

                return await next(invocationContext);
            };
        });

        return group;
    }
}
