using Microsoft.EntityFrameworkCore;
using SkillBridge.Api.Contracts.Mentors;
using SkillBridge.Api.Data;
using SkillBridge.Api.Services.Interfaces;

namespace SkillBridge.Api.Services;

public sealed class MentorConnectionService(AppDbContext db) : IMentorConnectionService
{
    public async Task<(bool Success, string? Error, MentorConnectionResponse? Connection)> CreateRequestAsync(
        CreateMentorConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var mentor = await db.Users
            .AsNoTracking()
            .Where(x => x.Id == request.MentorId && x.Role == "mentor" && x.IsActive)
            .Select(x => new { x.Id, x.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (mentor is null)
        {
            return (false, "Mentor not found.", null);
        }

        var email = request.MenteeEmail.Trim().ToLowerInvariant();
        var duplicatePending = await db.MentorConnections
            .AsNoTracking()
            .AnyAsync(x =>
                x.MentorId == mentor.Id &&
                x.MenteeEmail == email &&
                x.Status == "pending", cancellationToken);

        if (duplicatePending)
        {
            return (false, "You already have a pending request for this mentor.", null);
        }

        var item = new Models.MentorConnection
        {
            MentorId = mentor.Id,
            MenteeName = request.MenteeName.Trim(),
            MenteeEmail = email,
            MenteeGoal = request.MenteeGoal?.Trim(),
            Message = request.Message?.Trim(),
            Status = "pending"
        };

        db.MentorConnections.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return (true, null, new MentorConnectionResponse
        {
            Id = item.Id,
            MentorId = mentor.Id,
            MentorName = mentor.FullName,
            MenteeName = item.MenteeName,
            MenteeEmail = item.MenteeEmail,
            MenteeGoal = item.MenteeGoal,
            Message = item.Message,
            Status = item.Status,
            RequestedAt = item.RequestedAt
        });
    }

    public async Task<IReadOnlyList<MentorConnectionResponse>> GetRequestsForMenteeAsync(
        string menteeEmail,
        CancellationToken cancellationToken = default)
    {
        var email = menteeEmail.Trim().ToLowerInvariant();
        return await db.MentorConnections
            .AsNoTracking()
            .Include(x => x.Mentor)
            .Where(x => x.MenteeEmail == email)
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new MentorConnectionResponse
            {
                Id = x.Id,
                MentorId = x.MentorId,
                MentorName = x.Mentor.FullName,
                MenteeName = x.MenteeName,
                MenteeEmail = x.MenteeEmail,
                MenteeGoal = x.MenteeGoal,
                Message = x.Message,
                Status = x.Status,
                RequestedAt = x.RequestedAt
            })
            .ToListAsync(cancellationToken);
    }
}
