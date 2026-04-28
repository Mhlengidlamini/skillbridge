using SkillBridge.Api.Contracts.Mentors;
using SkillBridge.Api.Services.Interfaces;

namespace SkillBridge.Api.Endpoints;

public static class MentorConnectionEndpoints
{
    public static IEndpointRouteBuilder MapMentorConnectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/mentor-connections")
            .WithTags("Mentor Connections");

        group.MapPost("/", async (
            CreateMentorConnectionRequest request,
            IMentorConnectionService connectionService,
            CancellationToken cancellationToken = default) =>
        {
            if (request.MentorId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.MenteeName) ||
                string.IsNullOrWhiteSpace(request.MenteeEmail))
            {
                return Results.BadRequest(new { message = "MentorId, mentee name and mentee email are required." });
            }

            var result = await connectionService.CreateRequestAsync(request, cancellationToken);
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Error });
            }

            return Results.Created($"/api/mentor-connections/{result.Connection!.Id}", result.Connection);
        });

        group.MapGet("/mentee", async (
            string menteeEmail,
            IMentorConnectionService connectionService,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrWhiteSpace(menteeEmail))
            {
                return Results.BadRequest(new { message = "menteeEmail query is required." });
            }

            var items = await connectionService.GetRequestsForMenteeAsync(menteeEmail, cancellationToken);
            return Results.Ok(items);
        });

        return app;
    }
}
