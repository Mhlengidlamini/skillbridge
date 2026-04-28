using SkillBridge.Api.Contracts.Jobs;
using SkillBridge.Api.Services.Interfaces;

namespace SkillBridge.Api.Endpoints;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jobs")
            .WithTags("Jobs");

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
            if (string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Description) ||
                string.IsNullOrWhiteSpace(request.Type) ||
                string.IsNullOrWhiteSpace(request.EmployerName) ||
                string.IsNullOrWhiteSpace(request.EmployerEmail))
            {
                return Results.BadRequest(new { message = "Employer name, employer email, title, description and type are required." });
            }

            var result = await jobService.CreateJobAsync(request, cancellationToken);
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Error });
            }

            return Results.Created($"/api/jobs/{result.Job!.Id}", result.Job);
        });

        group.MapPost("/{jobId:guid}/match-score", async (
            Guid jobId,
            JobMatchScoreRequest request,
            IJobService jobService,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrWhiteSpace(request.CandidateSkills))
            {
                return Results.BadRequest(new { message = "CandidateSkills is required for scoring." });
            }

            var score = await jobService.GetJobMatchScoreAsync(jobId, request, cancellationToken);
            return score is null
                ? Results.NotFound(new { message = "Job not found." })
                : Results.Ok(score);
        });

        return app;
    }
}
