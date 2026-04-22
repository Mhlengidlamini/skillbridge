using System.Security.Claims;
using SkillBridge.Api.Auth;
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

        group.MapGet("/employer/{employerId:guid}", async (
            Guid employerId,
            ClaimsPrincipal user,
            IJobService jobService,
            CancellationToken cancellationToken = default) =>
        {
            if (!user.TryGetUserId(out var userId))
            {
                return Results.Unauthorized();
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && userId != employerId)
            {
                return Results.Forbid();
            }

            var jobs = await jobService.GetEmployerJobsAsync(employerId, cancellationToken);
            return Results.Ok(jobs);
        })
            .RequireAuthorization("EmployerOrAdmin");

        group.MapPost("/", async (
            CreateJobRequest request,
            ClaimsPrincipal user,
            IJobService jobService,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Description) ||
                string.IsNullOrWhiteSpace(request.Type))
            {
                return Results.BadRequest(new { message = "Title, description and type are required." });
            }

            if (!user.TryGetUserId(out var employerUserId))
            {
                return Results.Unauthorized();
            }

            var result = await jobService.CreateJobAsync(request, employerUserId, cancellationToken);
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Error });
            }

            return Results.Created($"/api/jobs/{result.Job!.Id}", result.Job);
        })
            .RequireAuthorization("EmployerOrAdmin");

        return app;
    }
}
