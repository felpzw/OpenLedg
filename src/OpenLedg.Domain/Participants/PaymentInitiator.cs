namespace OpenLedg.Domain.Participants;

public sealed class PaymentInitiator : BaseParticipant
{
    private PaymentInitiator() { }

    public PaymentInitiator(string organizationId, string displayName, string idempotencyKey)
        : base(ParticipantKind.PaymentInitiator, idempotencyKey)
    {
        OrganizationId = Guard.Text(organizationId, 200, nameof(organizationId));
        DisplayName = Guard.Text(displayName, 200, nameof(displayName));
    }

    public string OrganizationId { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
}
