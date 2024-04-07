using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class LatesttoMapper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "City",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "EducationOrganizations");

            migrationBuilder.RenameColumn(
                name: "StreetNumberName",
                table: "EducationOrganizations",
                newName: "Contacts");

            migrationBuilder.RenameColumn(
                name: "StateAbbreviation",
                table: "EducationOrganizations",
                newName: "Address");

            migrationBuilder.AlterColumn<Guid>(
                name: "EducationOrganizationId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncomingOutgoing",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "EducationOrganizations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations",
                column: "Domain",
                unique: true,
                filter: "[Domain] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests",
                column: "EducationOrganizationId",
                principalTable: "EducationOrganizations",
                principalColumn: "EducationOrganizationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "IncomingOutgoing",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Payload",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "EducationOrganizations");

            migrationBuilder.RenameColumn(
                name: "Contacts",
                table: "EducationOrganizations",
                newName: "StreetNumberName");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "EducationOrganizations",
                newName: "StateAbbreviation");

            migrationBuilder.AlterColumn<Guid>(
                name: "EducationOrganizationId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EducationOrganizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "EducationOrganizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests",
                column: "EducationOrganizationId",
                principalTable: "EducationOrganizations",
                principalColumn: "EducationOrganizationId");
        }
    }
}
