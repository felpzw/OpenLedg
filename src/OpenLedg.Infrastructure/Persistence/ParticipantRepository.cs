using Microsoft.EntityFrameworkCore;
using OpenLedg.Application.Abstractions;
using OpenLedg.Domain.Participants;

namespace OpenLedg.Infrastructure.Persistence;

public sealed class ParticipantRepository(OlgpDbContext db) : IParticipantRepository
{
    public Task<BaseParticipant?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Participants.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<BaseParticipant?> FindByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default)
        => db.Participants.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);

    public async Task AddAsync(BaseParticipant participant, CancellationToken cancellationToken = default)
        => await db.Participants.AddAsync(participant, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
