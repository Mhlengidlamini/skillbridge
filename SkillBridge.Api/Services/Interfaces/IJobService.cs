using SkillBridge.Api.Contracts.Jobs;

namespace SkillBridge.Api.Services.Interfaces;

public interface IJobService
{
    Task<IReadOnlyList<JobResponse>> GetPublicJobsAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobResponse>> GetEmployerJobsAsync(Guid employerId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, JobResponse? Job)> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default);
}
