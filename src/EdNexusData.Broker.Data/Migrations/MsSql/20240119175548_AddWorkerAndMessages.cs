using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class AddWorkerAndMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload_PayloadDirection",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.DropColumn(
                name: "PayloadDirection",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "EducationOrganizationPayloadSettings",
                newName: "OutgoingPayloadSettings");

            migrationBuilder.AddColumn<string>(
                name: "ProcessState",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerInstance",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "PayloadContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MessageTimestamp",
                table: "Messages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "IncomingPayloadSettings",
                table: "EducationOrganizationPayloadSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload" },
                unique: true,
                filter: "[EducationOrganizationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.DropColumn(
                name: "ProcessState",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WorkerInstance",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "PayloadContents");

            migrationBuilder.DropColumn(
                name: "IncomingPayloadSettings",
                table: "EducationOrganizationPayloadSettings");

            migrationBuilder.RenameColumn(
                name: "OutgoingPayloadSettings",
                table: "EducationOrganizationPayloadSettings",
                newName: "Settings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MessageTimestamp",
                table: "Messages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayloadDirection",
                table: "EducationOrganizationPayloadSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload_PayloadDirection",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload", "PayloadDirection" },
                unique: true,
                filter: "[EducationOrganizationId] IS NOT NULL AND [PayloadDirection] IS NOT NULL");
        }
    }
}
