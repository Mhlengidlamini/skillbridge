using Microsoft.EntityFrameworkCore;
using SkillBridge.Api.Contracts.Jobs;
using SkillBridge.Api.Data;
using SkillBridge.Api.Models;
using SkillBridge.Api.Services.Interfaces;

namespace SkillBridge.Api.Services;

public sealed class JobService(AppDbContext db) : IJobService
{
    public async Task<IReadOnlyList<JobResponse>> GetPublicJobsAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = db.Jobs
            .AsNoTracking()
            .Include(x => x.Employer)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapToResponse())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobResponse>> GetEmployerJobsAsync(Guid employerId, CancellationToken cancellationToken = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Include(x => x.Employer)
            .Where(x => x.EmployerId == employerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapToResponse())
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, JobResponse? Job)> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.EmployerEmail.Trim().ToLowerInvariant();
        var normalizedName = request.EmployerName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(normalizedName))
        {
            return (false, "Employer name and email are required.", null);
        }

        var employer = await db.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (employer is null)
        {
            employer = new User
            {
                FullName = normalizedName,
                Email = normalizedEmail,
                Role = "employer",
                IsActive = true
            };

            db.Users.Add(employer);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!string.Equals(employer.Role, "employer", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(employer.Role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Only employer or admin accounts can post jobs.", null);
        }

        var job = new Job
        {
            EmployerId = employer.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type.Trim().ToLowerInvariant(),
            Location = request.Location?.Trim(),
            IsRemote = request.IsRemote
        };

        db.Jobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        var created = await db.Jobs
            .AsNoTracking()
            .Include(x => x.Employer)
            .Where(x => x.Id == job.Id)
            .Select(MapToResponse())
            .FirstAsync(cancellationToken);

        return (true, null, created);
    }

    public async Task<JobMatchScoreResponse?> GetJobMatchScoreAsync(Guid jobId, JobMatchScoreRequest request, CancellationToken cancellationToken = default)
    {
        var job = await db.Jobs
            .AsNoTracking()
            .Where(x => x.Id == jobId)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.Type,
                x.Location,
                x.IsRemote
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            return null;
        }

        var score = 30;
        var signals = new List<string>();

        var candidateSkills = Tokenize(request.CandidateSkills);
        var targetRole = request.TargetRole?.Trim().ToLowerInvariant();
        var preferredLocation = request.PreferredLocation?.Trim().ToLowerInvariant();
        var jobText = $"{job.Title} {job.Description} {job.Type}".ToLowerInvariant();

        if (candidateSkills.Count > 0)
        {
            var matchedSkills = candidateSkills.Count(skill => jobText.Contains(skill, StringComparison.Ordinal));
            if (matchedSkills > 0)
            {
                score += Math.Min(45, matchedSkills * 9);
                signals.Add($"{matchedSkills} skill keyword match(es)");
            }
        }

        if (!string.IsNullOrWhiteSpace(targetRole) &&
            (job.Title.Contains(targetRole, StringComparison.OrdinalIgnoreCase) ||
             job.Description.Contains(targetRole, StringComparison.OrdinalIgnoreCase)))
        {
            score += 15;
            signals.Add("role alignment");
        }

        if (!string.IsNullOrWhiteSpace(preferredLocation))
        {
            if (job.IsRemote || (job.Location?.Contains(preferredLocation, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                score += 10;
                signals.Add("location preference fit");
            }
        }

        score = Math.Clamp(score, 0, 100);

        var summary = signals.Count == 0
            ? "Low alignment detected. Add more candidate skills for better scoring."
            : $"Match driven by {string.Join(", ", signals)}.";

        return new JobMatchScoreResponse
        {
            JobId = job.Id,
            Score = score,
            Summary = summary
        };
    }

    private static System.Linq.Expressions.Expression<Func<Job, JobResponse>> MapToResponse() => x => new JobResponse
    {
        Id = x.Id,
        EmployerId = x.EmployerId,
        EmployerName = x.Employer.FullName,
        EmployerEmail = x.Employer.Email,
        Title = x.Title,
        Description = x.Description,
        Type = x.Type,
        Location = x.Location,
        IsRemote = x.IsRemote,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt
    };

    private static HashSet<string> Tokenize(string value)
    {
        return value
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Length > 1)
            .ToHashSet();
    }
}
