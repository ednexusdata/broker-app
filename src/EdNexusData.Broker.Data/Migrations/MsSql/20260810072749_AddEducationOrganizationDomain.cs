using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class AddEducationOrganizationDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLog_Requests_RequestId",
                table: "ActivityLog");

            migrationBuilder.CreateTable(
                name: "EducationOrganizationDomains",
                columns: table => new
                {
                    EducationOrganizationDomainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizationDomains", x => x.EducationOrganizationDomainId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizationDomains_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationDomains_Domain",
                table: "EducationOrganizationDomains",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationDomains_EducationOrganizationId",
                table: "EducationOrganizationDomains",
                column: "EducationOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLog_Requests_RequestId",
                table: "ActivityLog",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);
            
            // Preserve each district's existing single Domain value as its first row in the new table
            // before the old column is dropped below.
            migrationBuilder.Sql(@"
                INSERT INTO [EducationOrganizationDomains] ([EducationOrganizationDomainId], [EducationOrganizationId], [Domain], [CreatedAt])
                SELECT NEWID(), [EducationOrganizationId], [Domain], SYSDATETIMEOFFSET()
                FROM [EducationOrganizations]
                WHERE [Domain] IS NOT NULL AND [Domain] <> '';
            ");

            migrationBuilder.DropIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "EducationOrganizations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLog_Requests_RequestId",
                table: "ActivityLog");

            migrationBuilder.DropTable(
                name: "EducationOrganizationDomains");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "EducationOrganizations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations",
                column: "Domain",
                unique: true,
                filter: "[Domain] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLog_Requests_RequestId",
                table: "ActivityLog",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
