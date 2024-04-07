using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AllOrganizationPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllEducationOrganizations",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllEducationOrganizations",
                table: "Users");
        }
    }
}
