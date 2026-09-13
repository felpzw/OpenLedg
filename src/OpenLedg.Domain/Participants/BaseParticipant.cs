namespace OpenLedg.Domain.Participants;

public abstract class BaseParticipant
{
    protected BaseParticipant() { }

    protected BaseParticipant(ParticipantKind kind, string idempotencyKey)
    {
        Id = Guid.NewGuid();
        Kind = kind;
        CreatedAt = DateTimeOffset.UtcNow;
        IdempotencyKey = Guard.Text(idempotencyKey, 128, nameof(idempotencyKey));
    }

    public Guid Id { get; private set; }
    public ParticipantKind Kind { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public ComplianceStatus KycStatus { get; private set; } = ComplianceStatus.Pending;
    public ComplianceStatus AmlStatus { get; private set; } = ComplianceStatus.Pending;
    public string IdempotencyKey { get; private set; } = null!;
}
