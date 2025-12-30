using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class AddEdOrgCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CeebCode",
                table: "EducationOrganizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NcesCode",
                table: "EducationOrganizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateCode",
                table: "EducationOrganizations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CeebCode",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "NcesCode",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "StateCode",
                table: "EducationOrganizations");
        }
    }
}
