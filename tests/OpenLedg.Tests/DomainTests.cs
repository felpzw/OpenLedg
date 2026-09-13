using System.Security.Cryptography;
using OpenLedg.Domain.Participants;
using OpenLedg.Domain.Routing;
using OpenLedg.Domain.Security;

namespace OpenLedg.Tests;

public sealed class DomainTests
{
    [Fact]
    public void New_participant_has_uuid_v4_and_pending_compliance()
    {
        var client = new IndividualClient("123.456.789-09", "Test Person", "request-1");
        Assert.Equal('4', client.Id.ToString()[14]);
        Assert.Equal("12345678909", client.Cpf);
        Assert.Equal(ComplianceStatus.Pending, client.KycStatus);
        Assert.Equal(ComplianceStatus.Pending, client.AmlStatus);
        Assert.Equal(TimeSpan.Zero, client.CreatedAt.Offset);
    }

    [Fact]
    public void Payment_initiator_cannot_receive_a_settlement_route()
    {
        var initiator = new PaymentInitiator("test-itp", "Test ITP", "request-2");
        Assert.Throws<InvalidOperationException>(() => new SettlementRoute(initiator, "1"));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("340282366920938463463374607431768211455")]
    [InlineData("340282366920938463463374607431768211456")]
    public void Invalid_ledger_identifiers_are_rejected(string id)
    {
        var client = new CorporateClient("12.345.678/0001-95", "Test Company", "request-3");
        Assert.Throws<ArgumentException>(() => new SettlementRoute(client, id));
    }

    [Fact]
    public void Ledger_identifier_preserves_all_128_bits()
    {
        const string id = "340282366920938463463374607431768211454";
        var client = new IndividualClient("12345678909", "Test Person", "request-4");
        Assert.Equal(id, new SettlementRoute(client, id).LedgerAccountId);
    }

    [Fact]
    public void Webhooks_require_https_and_public_ecdsa_keys()
    {
        var client = new CorporateClient("12345678000195", "Test Company", "request-5");
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        Assert.Throws<ArgumentException>(() => new WebhookConfiguration(client, "http://example.test", key.ExportSubjectPublicKeyInfoPem()));
        Assert.Throws<ArgumentException>(() => new WebhookConfiguration(client, "https://example.test", key.ExportECPrivateKeyPem()));
        var webhook = new WebhookConfiguration(client, "https://example.test/callback", key.ExportSubjectPublicKeyInfoPem());
        Assert.Equal(client.Id, webhook.ParticipantId);
    }

    [Fact]
    public void Domain_and_application_do_not_reference_infrastructure()
    {
        var domain = typeof(BaseParticipant).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(domain, x => x.Name!.Contains("EntityFramework") || x.Name.StartsWith("Npgsql") || x.Name.StartsWith("OpenLedg."));
        var application = typeof(Application.Abstractions.IParticipantRepository).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(application, x => x.Name is "OpenLedg.Infrastructure" or "OpenLedg.Api" || x.Name!.Contains("EntityFramework"));
    }
}
