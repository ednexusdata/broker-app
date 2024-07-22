using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_PayloadContents_PayloadContentId",
                table: "Mappings");

            migrationBuilder.DropIndex(
                name: "IX_Mappings_PayloadContentId",
                table: "Mappings");

            migrationBuilder.DropColumn(
                name: "Actions",
                table: "PayloadContents");

            migrationBuilder.RenameColumn(
                name: "SourceMapping",
                table: "Mappings",
                newName: "JsonSourceMapping");

            migrationBuilder.RenameColumn(
                name: "PayloadContentId",
                table: "Mappings",
                newName: "PayloadContentActionId");

            migrationBuilder.RenameColumn(
                name: "DestinationMapping",
                table: "Mappings",
                newName: "JsonDestinationMapping");

            migrationBuilder.AddColumn<byte>(
                name: "Version",
                table: "Mappings",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateTable(
                name: "PayloadContentActions",
                columns: table => new
                {
                    PayloadContentActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayloadContentActionType = table.Column<string>(type: "text", nullable: true),
                    ActiveMappingId = table.Column<Guid>(type: "uuid", nullable: true),
                    Settings = table.Column<string>(type: "jsonb", nullable: true),
                    Process = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayloadContentActions", x => x.PayloadContentActionId);
                    table.ForeignKey(
                        name: "FK_PayloadContentActions_Mappings_ActiveMappingId",
                        column: x => x.ActiveMappingId,
                        principalTable: "Mappings",
                        principalColumn: "MappingId");
                    table.ForeignKey(
                        name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                        column: x => x.PayloadContentId,
                        principalTable: "PayloadContents",
                        principalColumn: "PayloadContentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_PayloadContentActionId_Version",
                table: "Mappings",
                columns: new[] { "PayloadContentActionId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_ActiveMappingId",
                table: "PayloadContentActions",
                column: "ActiveMappingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentAction~",
                table: "PayloadContentActions",
                columns: new[] { "PayloadContentId", "PayloadContentActionType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_PayloadContentActions_PayloadContentActionId",
                table: "Mappings",
                column: "PayloadContentActionId",
                principalTable: "PayloadContentActions",
                principalColumn: "PayloadContentActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_PayloadContentActions_PayloadContentActionId",
                table: "Mappings");

            migrationBuilder.DropTable(
                name: "PayloadContentActions");

            migrationBuilder.DropIndex(
                name: "IX_Mappings_PayloadContentActionId_Version",
                table: "Mappings");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Mappings");

            migrationBuilder.RenameColumn(
                name: "PayloadContentActionId",
                table: "Mappings",
                newName: "PayloadContentId");

            migrationBuilder.RenameColumn(
                name: "JsonSourceMapping",
                table: "Mappings",
                newName: "SourceMapping");

            migrationBuilder.RenameColumn(
                name: "JsonDestinationMapping",
                table: "Mappings",
                newName: "DestinationMapping");

            migrationBuilder.AddColumn<string>(
                name: "Actions",
                table: "PayloadContents",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_PayloadContentId",
                table: "Mappings",
                column: "PayloadContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_PayloadContents_PayloadContentId",
                table: "Mappings",
                column: "PayloadContentId",
                principalTable: "PayloadContents",
                principalColumn: "PayloadContentId");
        }
    }
}
