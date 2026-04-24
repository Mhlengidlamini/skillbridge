using Microsoft.EntityFrameworkCore;
using SkillBridge.Api.Contracts.Users;
using SkillBridge.Api.Data;
using SkillBridge.Api.Models;
using SkillBridge.Api.Validation;

namespace SkillBridge.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithRequestValidation();

        group.MapGet("/", async (AppDbContext db) =>
            await db.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync());

        group.MapPost("/", async (CreateUserRequest request, AppDbContext db) =>
        {
            var exists = await db.Users.AnyAsync(x => x.Email == request.Email);
            if (exists)
            {
                return Results.Conflict(new { message = "Email already exists." });
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Phone = request.Phone?.Trim(),
                Location = request.Location?.Trim(),
                Bio = request.Bio?.Trim(),
                Education = request.Education?.Trim(),
                CareerGoal = request.CareerGoal?.Trim(),
                Role = request.Role.Trim().ToLowerInvariant()
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}", user);
        });

        return app;
    }
}
