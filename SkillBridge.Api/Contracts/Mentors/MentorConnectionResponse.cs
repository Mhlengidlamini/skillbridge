namespace SkillBridge.Api.Contracts.Mentors;

public sealed class MentorConnectionResponse
{
    public Guid Id { get; init; }
    public Guid MentorId { get; init; }
    public string MentorName { get; init; } = string.Empty;
    public string MenteeName { get; init; } = string.Empty;
    public string MenteeEmail { get; init; } = string.Empty;
    public string? MenteeGoal { get; init; }
    public string? Message { get; init; }
    public string Status { get; init; } = "pending";
    public DateTime RequestedAt { get; init; }
}
