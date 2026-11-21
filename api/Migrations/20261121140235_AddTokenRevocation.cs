using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenRevocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "mcp_tokens_revoked_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "revoked_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jti = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_revoked_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_revoked_tokens_expires_at",
                table: "revoked_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_revoked_tokens_jti",
                table: "revoked_tokens",
                column: "jti",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "revoked_tokens");

            migrationBuilder.DropColumn(
                name: "mcp_tokens_revoked_at",
                table: "users");
        }
    }
}
