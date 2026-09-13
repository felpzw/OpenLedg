using System.Security.Cryptography.X509Certificates;
using OpenLedg.Domain.Participants;

namespace OpenLedg.Domain.Security;

public sealed class InitiatorIntegration : ParticipantLink
{
    private InitiatorIntegration() { }
    public InitiatorIntegration(PaymentInitiator participant, string clientId, string tokenEndpoint,
        string mtlsCertificatePem, string privateKeyReference, string dpopPublicJwksUri) : base(participant)
    {
        ClientId = Guard.Text(clientId, 200, nameof(clientId));
        TokenEndpoint = Guard.Https(tokenEndpoint, nameof(tokenEndpoint));
        MtlsCertificatePem = Guard.Text(mtlsCertificatePem, 16384, nameof(mtlsCertificatePem));
        if (MtlsCertificatePem.Contains("PRIVATE KEY", StringComparison.Ordinal))
            throw new ArgumentException("Only the public certificate may be stored.", nameof(mtlsCertificatePem));
        using var certificate = X509Certificate2.CreateFromPem(MtlsCertificatePem);
        CertificateExpiresAt = new DateTimeOffset(certificate.NotAfter.ToUniversalTime());
        PrivateKeyReference = Guard.Text(privateKeyReference, 512, nameof(privateKeyReference));
        DpopPublicJwksUri = Guard.Https(dpopPublicJwksUri, nameof(dpopPublicJwksUri));
    }

    public string ClientId { get; private set; } = null!;
    public string TokenEndpoint { get; private set; } = null!;
    public string MtlsCertificatePem { get; private set; } = null!;
    public DateTimeOffset CertificateExpiresAt { get; private set; }
    public string PrivateKeyReference { get; private set; } = null!;
    public string DpopPublicJwksUri { get; private set; } = null!;
    public string DpopAlgorithm { get; private set; } = "ES256";
}
