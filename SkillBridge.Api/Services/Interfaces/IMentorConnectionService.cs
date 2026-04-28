using SkillBridge.Api.Contracts.Mentors;

namespace SkillBridge.Api.Services.Interfaces;

public interface IMentorConnectionService
{
    Task<(bool Success, string? Error, MentorConnectionResponse? Connection)> CreateRequestAsync(
        CreateMentorConnectionRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MentorConnectionResponse>> GetRequestsForMenteeAsync(
        string menteeEmail,
        CancellationToken cancellationToken = default);
}
