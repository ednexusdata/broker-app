using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class EdOrgPayloadSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationOrganizationPayloadSettings",
                columns: table => new
                {
                    EducationOrganizationPayloadSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayloadDirection = table.Column<int>(type: "integer", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Settings = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizationPayloadSettings", x => x.EducationOrganizationPayloadSettingsId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizationPayloadSettings_EducationOrganizations~",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationI~",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationOrganizationPayloadSettings");
        }
    }
}
