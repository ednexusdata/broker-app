using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EdOrgAddressContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EducationOrganizations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "EducationOrganizations",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "EducationOrganizations");

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
    }
}
