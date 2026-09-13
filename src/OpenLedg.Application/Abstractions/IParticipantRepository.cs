using OpenLedg.Domain.Participants;

namespace OpenLedg.Application.Abstractions;

public interface IParticipantRepository
{
    Task<BaseParticipant?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BaseParticipant?> FindByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task AddAsync(BaseParticipant participant, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
