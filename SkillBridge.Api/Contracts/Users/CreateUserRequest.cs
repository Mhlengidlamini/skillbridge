namespace SkillBridge.Api.Contracts.Users;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string? Phone,
    string? Location,
    string? Bio,
    string? Education,
    string? CareerGoal,
    string Role = "youth");
