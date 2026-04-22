namespace SkillBridge.Api.Contracts.Jobs;

public sealed class JobResponse
{
    public Guid Id { get; init; }
    public Guid EmployerId { get; init; }
    public string EmployerName { get; init; } = string.Empty;
    public string EmployerEmail { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Location { get; init; }
    public bool IsRemote { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
