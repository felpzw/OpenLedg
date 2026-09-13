using OpenLedg.Domain.Participants;

namespace OpenLedg.Domain.Security;

public sealed class ParticipantPermission : ParticipantLink
{
    private ParticipantPermission() { }
    public ParticipantPermission(BaseParticipant participant, string permission) : base(participant)
        => Permission = Guard.Text(permission, 100, nameof(permission));

    public string Permission { get; private set; } = null!;
}
