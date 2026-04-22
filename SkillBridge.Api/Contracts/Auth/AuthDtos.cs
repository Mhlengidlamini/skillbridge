namespace SkillBridge.Api.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterEmployerRequest(string FullName, string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Email,
    string FullName,
    string Role);
