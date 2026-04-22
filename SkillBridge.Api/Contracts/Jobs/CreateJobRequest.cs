namespace SkillBridge.Api.Contracts.Jobs;

public sealed record CreateJobRequest(
    string EmployerName,
    string EmployerEmail,
    string Title,
    string Description,
    string Type,
    string? Location,
    bool IsRemote);
