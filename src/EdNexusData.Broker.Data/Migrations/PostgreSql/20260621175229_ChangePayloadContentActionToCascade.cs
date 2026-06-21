using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
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

            migrationBuilder.AlterColumn<Guid>(
                name: "PayloadContentId",
                table: "PayloadContentActions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<Guid>(
                name: "PayloadContentId",
                table: "PayloadContentActions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                table: "PayloadContentActions",
                column: "PayloadContentId",
                principalTable: "PayloadContents",
                principalColumn: "PayloadContentId");
        }
    }
}
