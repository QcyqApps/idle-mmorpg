using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleMmo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    google_sub = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    display_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_banned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: true),
                    character_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<int>(type: "integer", nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sim_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    server_seed = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_id = table.Column<long>(type: "bigint", nullable: true),
                    user_agent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_device_hash",
                schema: "public",
                table: "accounts",
                column: "device_hash",
                unique: true,
                filter: "\"device_hash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_google_sub",
                schema: "public",
                table: "accounts",
                column: "google_sub",
                unique: true,
                filter: "\"google_sub\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_account_id_created_at",
                schema: "public",
                table: "audit_log",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_action_created_at",
                schema: "public",
                table: "audit_log",
                columns: new[] { "action", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_created_at",
                schema: "public",
                table: "audit_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_account_id",
                schema: "public",
                table: "refresh_tokens",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_family_id",
                schema: "public",
                table: "refresh_tokens",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token_hash",
                schema: "public",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "public");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "public");
        }
    }
}
