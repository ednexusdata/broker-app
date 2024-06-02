using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PayloadContentActionToJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "PayloadContents");

            migrationBuilder.AddColumn<string>(
                name: "Actions",
                table: "PayloadContents",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actions",
                table: "PayloadContents");

            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "PayloadContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
