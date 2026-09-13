using Microsoft.EntityFrameworkCore;
using OpenLedg.Domain.Participants;
using OpenLedg.Domain.Routing;
using OpenLedg.Domain.Security;
using OpenLedg.Infrastructure.Persistence;

namespace OpenLedg.Tests;

public sealed class ModelTests
{
    [Fact]
    public void Tph_credentials_and_routes_have_the_expected_boundaries()
    {
        using var db = new OlgpDbContextFactory().CreateDbContext([]);
        var model = db.Model;
        foreach (var type in new[] { typeof(IndividualClient), typeof(CorporateClient), typeof(PaymentInitiator) })
            Assert.Equal("participants", model.FindEntityType(type)!.GetTableName());
        Assert.Equal("security", model.FindEntityType(typeof(AuthenticationProfile))!.GetSchema());
        Assert.Equal("security", model.FindEntityType(typeof(InitiatorIntegration))!.GetSchema());
        var route = model.FindEntityType(typeof(SettlementRoute))!;
        Assert.Contains(route.GetForeignKeys(), key => key.Properties.Select(x => x.Name).SequenceEqual(["ParticipantId", "ParticipantKind"]));
        Assert.DoesNotContain(model.GetEntityTypes().SelectMany(x => x.GetProperties()), x => x.Name.Contains("Balance", StringComparison.OrdinalIgnoreCase));
        var sql = db.Database.GenerateCreateScript();
        Assert.Contains("ck_settlement_routes_participant_kind", sql);
        Assert.Contains("ck_participants_shape", sql);
    }
}
