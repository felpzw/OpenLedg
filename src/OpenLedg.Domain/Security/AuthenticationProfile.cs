using OpenLedg.Domain.Participants;

namespace OpenLedg.Domain.Security;

// Authentication is delegated to an identity provider. Secrets live in a vault.
public sealed class AuthenticationProfile : ParticipantLink
{
    private AuthenticationProfile() { }
    public AuthenticationProfile(IndividualClient participant, string issuer, string subject,
        string? mfaSecretReference = null, string? webAuthnCredentialId = null) : base(participant)
    {
        Issuer = Guard.Https(issuer, nameof(issuer));
        Subject = Guard.Text(subject, 200, nameof(subject));
        MfaSecretReference = mfaSecretReference is null ? null : Guard.Text(mfaSecretReference, 512, nameof(mfaSecretReference));
        WebAuthnCredentialId = webAuthnCredentialId is null ? null : Guard.Text(webAuthnCredentialId, 2048, nameof(webAuthnCredentialId));
    }

    public string Issuer { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string? MfaSecretReference { get; private set; }
    public string? WebAuthnCredentialId { get; private set; }
}
