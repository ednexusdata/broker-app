using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddRequestStatusToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestStatus",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestProcessUserId",
                table: "Requests",
                column: "RequestProcessUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ResponseProcessUserId",
                table: "Requests",
                column: "ResponseProcessUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_RequestProcessUserId",
                table: "Requests",
                column: "RequestProcessUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_ResponseProcessUserId",
                table: "Requests",
                column: "ResponseProcessUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_RequestProcessUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_ResponseProcessUserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_RequestProcessUserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ResponseProcessUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RequestStatus",
                table: "Messages");
        }
    }
}
