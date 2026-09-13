namespace OpenLedg.Domain.Participants;

public sealed class LegalRepresentative : ParticipantLink
{
    private LegalRepresentative() { }
    public LegalRepresentative(CorporateClient participant, string cpf, string fullName, string role) : base(participant)
    {
        Cpf = Guard.Document(cpf, "^[0-9]{11}$", nameof(cpf));
        FullName = Guard.Text(fullName, 200, nameof(fullName));
        Role = Guard.Text(role, 100, nameof(role));
    }

    public string Cpf { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Role { get; private set; } = null!;
}
