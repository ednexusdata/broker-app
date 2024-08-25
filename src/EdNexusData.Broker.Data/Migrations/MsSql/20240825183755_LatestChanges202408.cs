using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class LatestChanges202408 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_Requests_RequestId",
                table: "Mappings");

            migrationBuilder.DropIndex(
                name: "IX_Mappings_RequestId",
                table: "Mappings");

            migrationBuilder.RenameColumn(
                name: "SourceMapping",
                table: "Mappings",
                newName: "JsonSourceMapping");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "Mappings",
                newName: "PayloadContentActionId");

            migrationBuilder.RenameColumn(
                name: "DestinationMapping",
                table: "Mappings",
                newName: "JsonInitialMapping");

            migrationBuilder.AddColumn<string>(
                name: "JsonDestinationMapping",
                table: "Mappings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Version",
                table: "Mappings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateTable(
                name: "__BrokerSeedsHistory",
                columns: table => new
                {
                    SeedId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SeedName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___BrokerSeedsHistory", x => x.SeedId);
                });

            migrationBuilder.CreateTable(
                name: "PayloadContentActions",
                columns: table => new
                {
                    PayloadContentActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayloadContentActionType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ActiveMappingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Process = table.Column<bool>(type: "bit", nullable: false),
                    PayloadContentActionStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Worker_Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StartDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FinishDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobStatus = table.Column<int>(type: "int", nullable: false),
                    WorkerInstance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkerState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worker_Jobs", x => x.JobId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_PayloadContentActionId_Version",
                table: "Mappings",
                columns: new[] { "PayloadContentActionId", "Version" },
                unique: true,
                filter: "[PayloadContentActionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_ActiveMappingId",
                table: "PayloadContentActions",
                column: "ActiveMappingId",
                unique: true,
                filter: "[ActiveMappingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentActionType",
                table: "PayloadContentActions",
                columns: new[] { "PayloadContentId", "PayloadContentActionType" },
                unique: true,
                filter: "[PayloadContentId] IS NOT NULL AND [PayloadContentActionType] IS NOT NULL");

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
                name: "__BrokerSeedsHistory");

            migrationBuilder.DropTable(
                name: "PayloadContentActions");

            migrationBuilder.DropTable(
                name: "Worker_Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Mappings_PayloadContentActionId_Version",
                table: "Mappings");

            migrationBuilder.DropColumn(
                name: "JsonDestinationMapping",
                table: "Mappings");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Mappings");

            migrationBuilder.RenameColumn(
                name: "PayloadContentActionId",
                table: "Mappings",
                newName: "RequestId");

            migrationBuilder.RenameColumn(
                name: "JsonSourceMapping",
                table: "Mappings",
                newName: "SourceMapping");

            migrationBuilder.RenameColumn(
                name: "JsonInitialMapping",
                table: "Mappings",
                newName: "DestinationMapping");

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_RequestId",
                table: "Mappings",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_Requests_RequestId",
                table: "Mappings",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "RequestId");
        }
    }
}
