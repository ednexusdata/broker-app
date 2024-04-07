using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEdOrgConnectorSettingsWithRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationOrganizationConnectorSettings",
                columns: table => new
                {
                    EducationOrganizationConnectorSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Connector = table.Column<string>(type: "text", nullable: false),
                    Settings = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizationConnectorSettings", x => x.EducationOrganizationConnectorSettingsId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizationConnectorSettings_EducationOrganizatio~",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Student = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    RequestManifest = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    RequestProcessUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitialRequestSentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseManifest = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ResponseProcessUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestResponse = table.Column<int>(type: "integer", nullable: false),
                    MessageTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MessageContents = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    TransmissionDetails = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayloadContents",
                columns: table => new
                {
                    PayloadContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: true),
                    BlobContent = table.Column<byte[]>(type: "bytea", nullable: true),
                    JsonContent = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    XmlContent = table.Column<string>(type: "xml", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayloadContents", x => x.PayloadContentId);
                    table.ForeignKey(
                        name: "FK_PayloadContents_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationConnectorSettings_EducationOrganizatio~",
                table: "EducationOrganizationConnectorSettings",
                columns: new[] { "EducationOrganizationId", "Connector" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RequestId",
                table: "Messages",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContents_MessageId",
                table: "PayloadContents",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_EducationOrganizationId",
                table: "Requests",
                column: "EducationOrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationOrganizationConnectorSettings");

            migrationBuilder.DropTable(
                name: "PayloadContents");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
