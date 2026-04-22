using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Api.Contracts.Auth;
using SkillBridge.Api.Data;
using SkillBridge.Api.Models;
using SkillBridge.Api.Services;

namespace SkillBridge.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register-employer", async (
            RegisterEmployerRequest request,
            AppDbContext db,
            JwtTokenService tokens,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Full name, email and password are required." });
            }

            if (request.Password.Length < 8)
            {
                return Results.BadRequest(new { message = "Password must be at least 8 characters." });
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var exists = await db.Users.AnyAsync(x => x.Email == email, cancellationToken);
            if (exists)
            {
                return Results.Conflict(new { message = "Email already exists." });
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                Role = "employer",
                IsActive = true
            };
            user.PasswordHash = hasher.HashPassword(user, request.Password);

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);

            var (token, expires) = tokens.CreateToken(user);
            return Results.Ok(new AuthResponse(token, expires, user.Id, user.Email, user.FullName, user.Role));
        });

        group.MapPost("/login", async (
            LoginRequest request,
            AppDbContext db,
            JwtTokenService tokens,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and password are required." });
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
            if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var hasher = new PasswordHasher<User>();
            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                return Results.Unauthorized();
            }

            if (!user.IsActive)
            {
                return Results.Json(new { message = "Account is disabled." }, statusCode: StatusCodes.Status403Forbidden);
            }

            var (token, expires) = tokens.CreateToken(user);
            return Results.Ok(new AuthResponse(token, expires, user.Id, user.Email, user.FullName, user.Role));
        });

        return app;
    }
}
