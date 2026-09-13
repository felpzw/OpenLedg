using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenLedg.Domain.Participants;
using OpenLedg.Domain.Routing;
using OpenLedg.Domain.Security;

namespace OpenLedg.Infrastructure.Persistence;

public sealed class OlgpDbContext(DbContextOptions<OlgpDbContext> options) : DbContext(options)
{
    public DbSet<BaseParticipant> Participants => Set<BaseParticipant>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema("olgp");
        var participant = model.Entity<BaseParticipant>();
        participant.ToTable("participants", table =>
        {
            table.HasCheckConstraint("ck_participants_uuid_v4", "substring(id::text, 15, 1) = '4'");
            table.HasCheckConstraint("ck_participants_kind", "kind IN ('Individual', 'Corporate', 'PaymentInitiator')");
            table.HasCheckConstraint("ck_participants_kyc", "kyc_status IN ('Pending', 'Approved', 'Rejected', 'Suspended')");
            table.HasCheckConstraint("ck_participants_aml", "aml_status IN ('Pending', 'Approved', 'Rejected', 'Suspended')");
            table.HasCheckConstraint("ck_participants_shape", """
                (kind = 'Individual' AND cpf IS NOT NULL AND cpf ~ '^[0-9]{11}$' AND full_name IS NOT NULL
                    AND cnpj IS NULL AND legal_name IS NULL AND organization_id IS NULL AND display_name IS NULL)
                OR (kind = 'Corporate' AND cnpj IS NOT NULL AND cnpj ~ '^[A-Z0-9]{12}[0-9]{2}$' AND legal_name IS NOT NULL
                    AND cpf IS NULL AND full_name IS NULL AND organization_id IS NULL AND display_name IS NULL)
                OR (kind = 'PaymentInitiator' AND organization_id IS NOT NULL AND display_name IS NOT NULL
                    AND cpf IS NULL AND full_name IS NULL AND cnpj IS NULL AND legal_name IS NULL)
                """);
        });
        participant.HasKey(x => x.Id);
        participant.HasAlternateKey(x => new { x.Id, x.Kind });
        participant.Property(x => x.Id).ValueGeneratedNever();
        participant.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        participant.HasDiscriminator(x => x.Kind)
            .HasValue<IndividualClient>(ParticipantKind.Individual)
            .HasValue<CorporateClient>(ParticipantKind.Corporate)
            .HasValue<PaymentInitiator>(ParticipantKind.PaymentInitiator);
        participant.Property(x => x.KycStatus).HasConversion<string>().HasMaxLength(32);
        participant.Property(x => x.AmlStatus).HasConversion<string>().HasMaxLength(32);
        participant.Property(x => x.IdempotencyKey).HasMaxLength(128);
        participant.HasIndex(x => x.IdempotencyKey).IsUnique();
        model.Entity<IndividualClient>().Property(x => x.Cpf).HasMaxLength(11);
        model.Entity<IndividualClient>().HasIndex(x => x.Cpf).IsUnique().HasFilter("cpf IS NOT NULL");
        model.Entity<CorporateClient>().Property(x => x.Cnpj).HasMaxLength(14);
        model.Entity<CorporateClient>().HasIndex(x => x.Cnpj).IsUnique().HasFilter("cnpj IS NOT NULL");
        model.Entity<PaymentInitiator>().HasIndex(x => x.OrganizationId).IsUnique().HasFilter("organization_id IS NOT NULL");

        var pix = Link<PixKey>(model, "pix_keys", "olgp", "'Individual'");
        pix.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        pix.Property(x => x.Value).HasMaxLength(256);
        pix.HasIndex(x => new { x.Kind, x.Value }).IsUnique();
        pix.HasIndex(x => x.DictReference).IsUnique();
        var representative = Link<LegalRepresentative>(model, "legal_representatives", "olgp", "'Corporate'");
        representative.Property(x => x.Cpf).HasMaxLength(11);
        representative.HasIndex(x => new { x.ParticipantId, x.Cpf }).IsUnique();

        var auth = Link<AuthenticationProfile>(model, "authentication_profiles", "security", "'Individual'");
        auth.Property(x => x.Issuer).HasMaxLength(2048);
        auth.Property(x => x.MfaSecretReference).HasMaxLength(512);
        auth.Property(x => x.WebAuthnCredentialId).HasMaxLength(2048);
        auth.HasIndex(x => new { x.Issuer, x.Subject }).IsUnique();
        auth.HasIndex(x => x.ParticipantId).IsUnique();

        var integration = Link<InitiatorIntegration>(model, "initiator_integrations", "security", "'PaymentInitiator'");
        integration.Property(x => x.MtlsCertificatePem).HasMaxLength(16384);
        integration.Property(x => x.PrivateKeyReference).HasMaxLength(512);
        integration.Property(x => x.TokenEndpoint).HasMaxLength(2048);
        integration.Property(x => x.DpopPublicJwksUri).HasMaxLength(2048);
        integration.HasIndex(x => x.ParticipantId).IsUnique();

        var webhook = Link<WebhookConfiguration>(model, "webhook_configurations", "security", "'Corporate'");
        webhook.Property(x => x.Url).HasMaxLength(2048);
        webhook.Property(x => x.VerificationPublicKeyPem).HasMaxLength(4096);
        webhook.HasIndex(x => new { x.ParticipantId, x.Url }).IsUnique();
        var permission = Link<ParticipantPermission>(model, "participant_permissions", "security");
        permission.Property(x => x.Permission).HasMaxLength(100);
        permission.HasIndex(x => new { x.ParticipantId, x.Permission }).IsUnique();

        var route = Link<SettlementRoute>(model, "settlement_routes", "routing", "'Individual', 'Corporate'");
        route.Property(x => x.LedgerAccountId).HasMaxLength(39);
        route.HasIndex(x => x.LedgerAccountId).IsUnique();
        route.ToTable("settlement_routes", "routing", table => table.HasCheckConstraint("ck_routes_account_id",
            "ledger_account_id ~ '^[1-9][0-9]{0,38}$' AND ledger_account_id::numeric < 340282366920938463463374607431768211455"));

        foreach (var entity in model.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(Regex.Replace(property.Name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant());
                if (property.ClrType == typeof(string) && property.GetMaxLength() is null)
                    property.SetMaxLength(200);
            }
        }
    }

    private static EntityTypeBuilder<T> Link<T>(ModelBuilder model, string table, string schema, string? kinds = null)
        where T : ParticipantLink
    {
        var entity = model.Entity<T>();
        entity.HasBaseType((Type?)null);
        entity.ToTable(table, schema, config =>
        {
            if (kinds is not null)
                config.HasCheckConstraint($"ck_{table}_participant_kind", $"participant_kind IN ({kinds})");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();
        entity.Property(x => x.ParticipantKind).HasConversion<string>().HasMaxLength(32);
        entity.HasOne<BaseParticipant>().WithMany()
            .HasForeignKey(x => new { x.ParticipantId, x.ParticipantKind })
            .HasPrincipalKey(x => new { x.Id, x.Kind }).OnDelete(DeleteBehavior.Restrict);
        return entity;
    }
}
