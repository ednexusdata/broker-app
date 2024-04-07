using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestToPayloadContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayloadContents_Messages_MessageId",
                table: "PayloadContents");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "PayloadContents",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "PayloadContents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContents_RequestId",
                table: "PayloadContents",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayloadContents_Messages_MessageId",
                table: "PayloadContents",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayloadContents_Requests_RequestId",
                table: "PayloadContents",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayloadContents_Messages_MessageId",
                table: "PayloadContents");

            migrationBuilder.DropForeignKey(
                name: "FK_PayloadContents_Requests_RequestId",
                table: "PayloadContents");

            migrationBuilder.DropIndex(
                name: "IX_PayloadContents_RequestId",
                table: "PayloadContents");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "PayloadContents");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "PayloadContents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayloadContents_Messages_MessageId",
                table: "PayloadContents",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
