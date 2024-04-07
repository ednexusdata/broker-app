using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EdOrgUniqueIndexForDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations",
                column: "Domain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations");
        }
    }
}
