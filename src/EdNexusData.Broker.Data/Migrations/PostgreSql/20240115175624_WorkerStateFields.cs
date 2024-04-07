using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class WorkerStateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessState",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerInstance",
                table: "Requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessState",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WorkerInstance",
                table: "Requests");
        }
    }
}
