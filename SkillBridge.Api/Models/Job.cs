namespace SkillBridge.Api.Models;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "internship";
    public string? Location { get; set; }
    public bool IsRemote { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Employer { get; set; } = null!;
}
