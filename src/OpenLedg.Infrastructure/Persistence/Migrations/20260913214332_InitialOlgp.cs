using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenLedg.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialOlgp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.EnsureSchema(
                name: "olgp");

            migrationBuilder.EnsureSchema(
                name: "routing");

            migrationBuilder.CreateTable(
                name: "participants",
                schema: "olgp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kyc_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    aml_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    organization_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participants", x => x.id);
                    table.UniqueConstraint("AK_participants_id_kind", x => new { x.id, x.kind });
                    table.CheckConstraint("ck_participants_aml", "aml_status IN ('Pending', 'Approved', 'Rejected', 'Suspended')");
                    table.CheckConstraint("ck_participants_kind", "kind IN ('Individual', 'Corporate', 'PaymentInitiator')");
                    table.CheckConstraint("ck_participants_kyc", "kyc_status IN ('Pending', 'Approved', 'Rejected', 'Suspended')");
                    table.CheckConstraint("ck_participants_shape", "(kind = 'Individual' AND cpf IS NOT NULL AND cpf ~ '^[0-9]{11}$' AND full_name IS NOT NULL\n    AND cnpj IS NULL AND legal_name IS NULL AND organization_id IS NULL AND display_name IS NULL)\nOR (kind = 'Corporate' AND cnpj IS NOT NULL AND cnpj ~ '^[A-Z0-9]{12}[0-9]{2}$' AND legal_name IS NOT NULL\n    AND cpf IS NULL AND full_name IS NULL AND organization_id IS NULL AND display_name IS NULL)\nOR (kind = 'PaymentInitiator' AND organization_id IS NOT NULL AND display_name IS NOT NULL\n    AND cpf IS NULL AND full_name IS NULL AND cnpj IS NULL AND legal_name IS NULL)");
                    table.CheckConstraint("ck_participants_uuid_v4", "substring(id::text, 15, 1) = '4'");
                });

            migrationBuilder.CreateTable(
                name: "authentication_profiles",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    issuer = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    mfa_secret_reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    web_authn_credential_id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authentication_profiles", x => x.id);
                    table.CheckConstraint("ck_authentication_profiles_participant_kind", "participant_kind IN ('Individual')");
                    table.ForeignKey(
                        name: "FK_authentication_profiles_participants_participant_id_partici~",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "initiator_integrations",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    token_endpoint = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    mtls_certificate_pem = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: false),
                    certificate_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    private_key_reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    dpop_public_jwks_uri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    dpop_algorithm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_initiator_integrations", x => x.id);
                    table.CheckConstraint("ck_initiator_integrations_participant_kind", "participant_kind IN ('PaymentInitiator')");
                    table.ForeignKey(
                        name: "FK_initiator_integrations_participants_participant_id_particip~",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "legal_representatives",
                schema: "olgp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_legal_representatives", x => x.id);
                    table.CheckConstraint("ck_legal_representatives_participant_kind", "participant_kind IN ('Corporate')");
                    table.ForeignKey(
                        name: "FK_legal_representatives_participants_participant_id_participa~",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "participant_permissions",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participant_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_participant_permissions_participants_participant_id_partici~",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pix_keys",
                schema: "olgp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    dict_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pix_keys", x => x.id);
                    table.CheckConstraint("ck_pix_keys_participant_kind", "participant_kind IN ('Individual')");
                    table.ForeignKey(
                        name: "FK_pix_keys_participants_participant_id_participant_kind",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "settlement_routes",
                schema: "routing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ledger_account_id = table.Column<string>(type: "character varying(39)", maxLength: 39, nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_routes", x => x.id);
                    table.CheckConstraint("ck_routes_account_id", "ledger_account_id ~ '^[1-9][0-9]{0,38}$' AND ledger_account_id::numeric < 340282366920938463463374607431768211455");
                    table.CheckConstraint("ck_settlement_routes_participant_kind", "participant_kind IN ('Individual', 'Corporate')");
                    table.ForeignKey(
                        name: "FK_settlement_routes_participants_participant_id_participant_k~",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "webhook_configurations",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    verification_public_key_pem = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_configurations", x => x.id);
                    table.CheckConstraint("ck_webhook_configurations_participant_kind", "participant_kind IN ('Corporate')");
                    table.ForeignKey(
                        name: "FK_webhook_configurations_participants_participant_id_particip~",
                        columns: x => new { x.participant_id, x.participant_kind },
                        principalSchema: "olgp",
                        principalTable: "participants",
                        principalColumns: new[] { "id", "kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authentication_profiles_issuer_subject",
                schema: "security",
                table: "authentication_profiles",
                columns: new[] { "issuer", "subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authentication_profiles_participant_id",
                schema: "security",
                table: "authentication_profiles",
                column: "participant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authentication_profiles_participant_id_participant_kind",
                schema: "security",
                table: "authentication_profiles",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_initiator_integrations_participant_id",
                schema: "security",
                table: "initiator_integrations",
                column: "participant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_initiator_integrations_participant_id_participant_kind",
                schema: "security",
                table: "initiator_integrations",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_legal_representatives_participant_id_cpf",
                schema: "olgp",
                table: "legal_representatives",
                columns: new[] { "participant_id", "cpf" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_legal_representatives_participant_id_participant_kind",
                schema: "olgp",
                table: "legal_representatives",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_participant_permissions_participant_id_participant_kind",
                schema: "security",
                table: "participant_permissions",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_participant_permissions_participant_id_permission",
                schema: "security",
                table: "participant_permissions",
                columns: new[] { "participant_id", "permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participants_cnpj",
                schema: "olgp",
                table: "participants",
                column: "cnpj",
                unique: true,
                filter: "cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_participants_cpf",
                schema: "olgp",
                table: "participants",
                column: "cpf",
                unique: true,
                filter: "cpf IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_participants_idempotency_key",
                schema: "olgp",
                table: "participants",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participants_organization_id",
                schema: "olgp",
                table: "participants",
                column: "organization_id",
                unique: true,
                filter: "organization_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pix_keys_dict_reference",
                schema: "olgp",
                table: "pix_keys",
                column: "dict_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pix_keys_kind_value",
                schema: "olgp",
                table: "pix_keys",
                columns: new[] { "kind", "value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pix_keys_participant_id_participant_kind",
                schema: "olgp",
                table: "pix_keys",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_settlement_routes_ledger_account_id",
                schema: "routing",
                table: "settlement_routes",
                column: "ledger_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_routes_participant_id_participant_kind",
                schema: "routing",
                table: "settlement_routes",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_configurations_participant_id_participant_kind",
                schema: "security",
                table: "webhook_configurations",
                columns: new[] { "participant_id", "participant_kind" });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_configurations_participant_id_url",
                schema: "security",
                table: "webhook_configurations",
                columns: new[] { "participant_id", "url" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authentication_profiles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "initiator_integrations",
                schema: "security");

            migrationBuilder.DropTable(
                name: "legal_representatives",
                schema: "olgp");

            migrationBuilder.DropTable(
                name: "participant_permissions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "pix_keys",
                schema: "olgp");

            migrationBuilder.DropTable(
                name: "settlement_routes",
                schema: "routing");

            migrationBuilder.DropTable(
                name: "webhook_configurations",
                schema: "security");

            migrationBuilder.DropTable(
                name: "participants",
                schema: "olgp");
        }
    }
}
