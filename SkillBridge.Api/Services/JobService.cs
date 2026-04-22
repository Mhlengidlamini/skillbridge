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

    public async Task<(bool Success, string? Error, JobResponse? Job)> CreateJobAsync(CreateJobRequest request, Guid employerUserId, CancellationToken cancellationToken = default)
    {
        var employer = await db.Users
            .FirstOrDefaultAsync(x => x.Id == employerUserId, cancellationToken);

        if (employer is null)
        {
            return (false, "Employer account not found.", null);
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
}
