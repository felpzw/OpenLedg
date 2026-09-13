namespace OpenLedg.Domain.Participants;

public sealed class PixKey : ParticipantLink
{
    private PixKey() { }
    public PixKey(IndividualClient participant, PixKeyKind kind, string value, string dictReference) : base(participant)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        Kind = kind;
        Value = Guard.Text(value, 256, nameof(value));
        DictReference = Guard.Text(dictReference, 200, nameof(dictReference));
    }

    public PixKeyKind Kind { get; private set; }
    public string Value { get; private set; } = null!;
    public string DictReference { get; private set; } = null!;
}
