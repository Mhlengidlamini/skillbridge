namespace SkillBridge.Api.Contracts.Jobs;

public sealed class JobMatchScoreResponse
{
    public Guid JobId { get; init; }
    public int Score { get; init; }
    public string Summary { get; init; } = string.Empty;
}
