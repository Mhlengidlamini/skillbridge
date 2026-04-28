namespace SkillBridge.Api.Contracts.Mentors;

public sealed class CreateMentorConnectionRequest
{
    public Guid MentorId { get; init; }
    public string MenteeName { get; init; } = string.Empty;
    public string MenteeEmail { get; init; } = string.Empty;
    public string? MenteeGoal { get; init; }
    public string? Message { get; init; }
}
