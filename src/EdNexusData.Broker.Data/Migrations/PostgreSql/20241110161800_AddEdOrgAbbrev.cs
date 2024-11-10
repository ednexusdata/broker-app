using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddEdOrgAbbrev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "EducationOrganizations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Worker_Jobs_CreatedBy",
                table: "Worker_Jobs",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Worker_Jobs_Users_CreatedBy",
                table: "Worker_Jobs",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worker_Jobs_Users_CreatedBy",
                table: "Worker_Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Worker_Jobs_CreatedBy",
                table: "Worker_Jobs");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "EducationOrganizations");
        }
    }
}
