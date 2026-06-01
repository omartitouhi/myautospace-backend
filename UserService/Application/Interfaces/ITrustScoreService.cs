using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface ITrustScoreService
{
    Task<TrustScore?> GetAsync(Guid authUserId);

    Task<TrustScore?> RecalculateAsync(Guid authUserId);
}
