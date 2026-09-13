using System.Globalization;
using OpenLedg.Domain.Participants;

namespace OpenLedg.Domain.Routing;

// A pointer to the ledger, never a balance or a copy of financial entries.
public sealed class SettlementRoute : ParticipantLink
{
    private SettlementRoute() { }
    public SettlementRoute(BaseParticipant participant, string ledgerAccountId) : base(participant)
    {
        if (participant.Kind is ParticipantKind.PaymentInitiator)
            throw new InvalidOperationException("Payment initiators cannot hold direct settlement accounts.");
        if (!UInt128.TryParse(ledgerAccountId, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            || id == 0 || id == UInt128.MaxValue)
            throw new ArgumentException("A valid TigerBeetle UInt128 account identifier is required.", nameof(ledgerAccountId));
        LedgerAccountId = id.ToString(CultureInfo.InvariantCulture);
    }

    public string LedgerAccountId { get; private set; } = null!;
}
