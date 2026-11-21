using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class HashWebRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Web refresh tokens are now stored hashed. The existing rows hold plaintext
            // tokens that would never match again (the web app doesn't use them anyway).
            migrationBuilder.Sql("DELETE FROM refresh_token;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deleted tokens can't be restored.
        }
    }
}
