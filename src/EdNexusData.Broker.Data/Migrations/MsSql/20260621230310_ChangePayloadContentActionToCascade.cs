using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class ChangePayloadContentActionToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                table: "PayloadContentActions");

            migrationBuilder.DropIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentActionType",
                table: "PayloadContentActions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PayloadContentId",
                table: "PayloadContentActions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentActionType",
                table: "PayloadContentActions",
                columns: new[] { "PayloadContentId", "PayloadContentActionType" },
                unique: true,
                filter: "[PayloadContentActionType] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                table: "PayloadContentActions",
                column: "PayloadContentId",
                principalTable: "PayloadContents",
                principalColumn: "PayloadContentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                table: "PayloadContentActions");

            migrationBuilder.DropIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentActionType",
                table: "PayloadContentActions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PayloadContentId",
                table: "PayloadContentActions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentActionType",
                table: "PayloadContentActions",
                columns: new[] { "PayloadContentId", "PayloadContentActionType" },
                unique: true,
                filter: "[PayloadContentId] IS NOT NULL AND [PayloadContentActionType] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                table: "PayloadContentActions",
                column: "PayloadContentId",
                principalTable: "PayloadContents",
                principalColumn: "PayloadContentId");
        }
    }
}
