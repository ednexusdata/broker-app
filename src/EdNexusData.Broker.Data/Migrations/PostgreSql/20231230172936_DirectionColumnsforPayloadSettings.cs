using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DirectionColumnsforPayloadSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.DropColumn(
                name: "PayloadDirection",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "EducationOrganizationPayloadSettings",
                newName: "OutgoingPayloadSettings");

            migrationBuilder.AddColumn<string>(
                name: "IncomingPayloadSettings",
                table: "EducationOrganizationPayloadSettings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.DropColumn(
                name: "IncomingPayloadSettings",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.RenameColumn(
                name: "OutgoingPayloadSettings",
                table: "EducationOrganizationPayloadSettings",
                newName: "Settings");

            migrationBuilder.AddColumn<int>(
                name: "PayloadDirection",
                table: "EducationOrganizationPayloadSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload", "PayloadDirection" },
                unique: true);
        }
    }
}
