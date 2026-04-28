namespace SkillBridge.Api.Models;

public class MentorConnection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MentorId { get; set; }
    public string MenteeName { get; set; } = string.Empty;
    public string MenteeEmail { get; set; } = string.Empty;
    public string? MenteeGoal { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public User Mentor { get; set; } = null!;
}
