using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class EdOrgPayloadSettingsUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload", "PayloadDirection" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload" },
                unique: true);
        }
    }
}
