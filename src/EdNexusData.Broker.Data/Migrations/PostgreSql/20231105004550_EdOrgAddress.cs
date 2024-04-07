using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class EdOrgAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "InitialRequestSentDate",
                table: "Requests",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MessageTimestamp",
                table: "Messages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateAbbreviation",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetNumberName",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "StateAbbreviation",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "StreetNumberName",
                table: "EducationOrganizations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "InitialRequestSentDate",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MessageTimestamp",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
