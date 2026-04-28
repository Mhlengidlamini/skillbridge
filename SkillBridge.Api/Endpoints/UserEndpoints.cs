using Microsoft.EntityFrameworkCore;
using SkillBridge.Api.Data;
using SkillBridge.Api.Models;

namespace SkillBridge.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/", async (AppDbContext db) =>
            await db.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync());

        group.MapGet("/mentors", async (AppDbContext db) =>
            await db.Users
                .Where(x => x.Role == "mentor" && x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync());

        group.MapPost("/", async (CreateUserRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new { message = "Full name and email are required." });
            }

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

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string? Phone,
    string? Location,
    string? Bio,
    string? Education,
    string? CareerGoal,
    string Role = "youth");
