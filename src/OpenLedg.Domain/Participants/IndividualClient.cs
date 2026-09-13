namespace OpenLedg.Domain.Participants;

public sealed class IndividualClient : BaseParticipant
{
    private IndividualClient() { }

    public IndividualClient(string cpf, string fullName, string idempotencyKey)
        : base(ParticipantKind.Individual, idempotencyKey)
    {
        Cpf = Guard.Document(cpf, "^[0-9]{11}$", nameof(cpf));
        FullName = Guard.Text(fullName, 200, nameof(fullName));
    }

    public string Cpf { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
}
