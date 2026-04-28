namespace SkillBridge.Api.Contracts.Jobs;

public sealed class JobMatchScoreRequest
{
    public string CandidateSkills { get; init; } = string.Empty;
    public string? TargetRole { get; init; }
    public string? PreferredLocation { get; init; }
}
