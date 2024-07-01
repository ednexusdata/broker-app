using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ChangeMappingToPayloadContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_Requests_RequestId",
                table: "Mappings");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "Mappings",
                newName: "PayloadContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Mappings_RequestId",
                table: "Mappings",
                newName: "IX_Mappings_PayloadContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_PayloadContents_PayloadContentId",
                table: "Mappings",
                column: "PayloadContentId",
                principalTable: "PayloadContents",
                principalColumn: "PayloadContentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_PayloadContents_PayloadContentId",
                table: "Mappings");

            migrationBuilder.RenameColumn(
                name: "PayloadContentId",
                table: "Mappings",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Mappings_PayloadContentId",
                table: "Mappings",
                newName: "IX_Mappings_RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_Requests_RequestId",
                table: "Mappings",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "RequestId");
        }
    }
}
