namespace OpenLedg.Domain.Participants;

public sealed class CorporateClient : BaseParticipant
{
    private CorporateClient() { }

    public CorporateClient(string cnpj, string legalName, string idempotencyKey)
        : base(ParticipantKind.Corporate, idempotencyKey)
    {
        Cnpj = Guard.Document(cnpj, "^[A-Z0-9]{12}[0-9]{2}$", nameof(cnpj));
        LegalName = Guard.Text(legalName, 200, nameof(legalName));
    }

    public string Cnpj { get; private set; } = null!;
    public string LegalName { get; private set; } = null!;
}
