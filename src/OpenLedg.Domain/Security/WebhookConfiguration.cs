using System.Security.Cryptography;
using OpenLedg.Domain.Participants;

namespace OpenLedg.Domain.Security;

public sealed class WebhookConfiguration : ParticipantLink
{
    private WebhookConfiguration() { }
    public WebhookConfiguration(CorporateClient participant, string url, string verificationPublicKeyPem) : base(participant)
    {
        Url = Guard.Https(url, nameof(url));
        VerificationPublicKeyPem = Guard.Text(verificationPublicKeyPem, 4096, nameof(verificationPublicKeyPem));
        if (!VerificationPublicKeyPem.StartsWith("-----BEGIN PUBLIC KEY-----", StringComparison.Ordinal)
            || VerificationPublicKeyPem.Contains("PRIVATE KEY", StringComparison.Ordinal))
            throw new ArgumentException("An ECDSA public key is required.", nameof(verificationPublicKeyPem));
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(VerificationPublicKeyPem);
    }

    public string Url { get; private set; } = null!;
    public string VerificationPublicKeyPem { get; private set; } = null!;
}
