namespace OpenLedg.Domain.Participants;

public abstract class ParticipantLink
{
    protected ParticipantLink() { }
    protected ParticipantLink(BaseParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);
        Id = Guid.NewGuid();
        ParticipantId = participant.Id;
        ParticipantKind = participant.Kind;
    }

    public Guid Id { get; private set; }
    public Guid ParticipantId { get; private set; }
    public ParticipantKind ParticipantKind { get; private set; }
}
