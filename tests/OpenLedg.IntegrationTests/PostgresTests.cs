using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenLedg.Domain.Participants;
using OpenLedg.Domain.Routing;
using OpenLedg.Domain.Security;
using OpenLedg.Infrastructure.Persistence;

namespace OpenLedg.IntegrationTests;

public sealed class PostgresTests
{
    private static string Required(string name) => Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"Set {name}. See docs/development.md to run integration tests.");

    private static OlgpDbContext Context() => new(new DbContextOptionsBuilder<OlgpDbContext>()
        .UseNpgsql(Required("OPENLEDG_TEST_OWNER"), options => options.MigrationsHistoryTable("__EFMigrationsHistory", "olgp")).Options);

    [Fact]
    public async Task Migration_and_tph_round_trip_preserve_participants_and_security_metadata()
    {
        await using var db = Context();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        await using var transaction = await db.Database.BeginTransactionAsync();
        var person = new IndividualClient("12345678909", "Test Person", Guid.NewGuid().ToString());
        var company = new CorporateClient("12345678000195", "Test Company", Guid.NewGuid().ToString());
        var itp = new PaymentInitiator(Guid.NewGuid().ToString(), "Test ITP", Guid.NewGuid().ToString());
        db.AddRange(person, company, itp);
        db.Add(new SettlementRoute(person, "340282366920938463463374607431768211454"));
        db.Add(new AuthenticationProfile(person, "https://identity.example.test", "test-subject", "vault://test/mfa", "test-webauthn-id"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.IsType<IndividualClient>(await db.Participants.SingleAsync(x => x.Id == person.Id));
        Assert.IsType<CorporateClient>(await db.Participants.SingleAsync(x => x.Id == company.Id));
        Assert.IsType<PaymentInitiator>(await db.Participants.SingleAsync(x => x.Id == itp.Id));
        Assert.Equal("vault://test/mfa", (await db.Set<AuthenticationProfile>().SingleAsync(x => x.ParticipantId == person.Id)).MfaSecretReference);
    }

    [Theory]
    [InlineData("PaymentInitiator", PostgresErrorCodes.CheckViolation)]
    [InlineData("Individual", PostgresErrorCodes.ForeignKeyViolation)]
    public async Task Database_rejects_itp_settlement_even_when_discriminator_is_forged(string kind, string expectedSqlState)
    {
        await using var db = Context();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var itp = new PaymentInitiator(Guid.NewGuid().ToString(), "Test ITP", Guid.NewGuid().ToString());
        db.Add(itp);
        await db.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO routing.settlement_routes (id, participant_id, participant_kind, ledger_account_id)
            VALUES ({Guid.NewGuid()}, {itp.Id}, {kind}, '42')
            """));
        Assert.Equal(expectedSqlState, exception.SqlState);
    }

    [Fact]
    public async Task Duplicate_idempotency_keys_are_rejected_by_database()
    {
        await using var db = Context();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var key = Guid.NewGuid().ToString();
        db.Add(new PaymentInitiator(Guid.NewGuid().ToString(), "First", key));
        await db.SaveChangesAsync();
        db.Add(new PaymentInitiator(Guid.NewGuid().ToString(), "Second", key));
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(exception.InnerException).SqlState);
    }

    [Fact]
    public async Task Runtime_role_reads_profiles_but_cannot_read_credentials_or_create_tables()
    {
        await using var connection = new NpgsqlConnection(Required("OPENLEDG_TEST_APP"));
        await connection.OpenAsync();
        await using (var command = new NpgsqlCommand("SELECT id FROM olgp.participants LIMIT 1", connection))
            await command.ExecuteNonQueryAsync();
        await using (var command = new NpgsqlCommand("SELECT * FROM security.authentication_profiles LIMIT 1", connection))
        {
            var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
        }
        await using var transaction = await connection.BeginTransactionAsync();
        await using var ddl = new NpgsqlCommand("CREATE TABLE olgp.forbidden (id int)", connection, transaction);
        var denied = await Assert.ThrowsAsync<PostgresException>(() => ddl.ExecuteNonQueryAsync());
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, denied.SqlState);
    }
}
