using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class WorkerJobsWithEdOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EducationOrganizationId",
                table: "Worker_Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Worker_Jobs_EducationOrganizationId",
                table: "Worker_Jobs",
                column: "EducationOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Worker_Jobs_EducationOrganizations_EducationOrganizationId",
                table: "Worker_Jobs",
                column: "EducationOrganizationId",
                principalTable: "EducationOrganizations",
                principalColumn: "EducationOrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worker_Jobs_EducationOrganizations_EducationOrganizationId",
                table: "Worker_Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Worker_Jobs_EducationOrganizationId",
                table: "Worker_Jobs");

            migrationBuilder.DropColumn(
                name: "EducationOrganizationId",
                table: "Worker_Jobs");
        }
    }
}
