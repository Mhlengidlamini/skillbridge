using SkillBridge.Api.Contracts.Jobs;
using SkillBridge.Api.Services.Interfaces;
using SkillBridge.Api.Validation;

namespace SkillBridge.Api.Endpoints;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jobs")
            .WithTags("Jobs")
            .WithRequestValidation();

        group.MapGet("/", async (IJobService jobService, bool activeOnly = true, CancellationToken cancellationToken = default) =>
        {
            var jobs = await jobService.GetPublicJobsAsync(activeOnly, cancellationToken);
            return Results.Ok(jobs);
        });

        group.MapGet("/employer/{employerId:guid}", async (Guid employerId, IJobService jobService, CancellationToken cancellationToken = default) =>
        {
            var jobs = await jobService.GetEmployerJobsAsync(employerId, cancellationToken);
            return Results.Ok(jobs);
        });

        group.MapPost("/", async (CreateJobRequest request, IJobService jobService, CancellationToken cancellationToken = default) =>
        {
            var result = await jobService.CreateJobAsync(request, cancellationToken);
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Error });
            }

            return Results.Created($"/api/jobs/{result.Job!.Id}", result.Job);
        });

        return app;
    }
}
