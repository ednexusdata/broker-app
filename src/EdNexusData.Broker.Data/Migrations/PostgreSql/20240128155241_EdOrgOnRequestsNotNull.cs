using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EdOrgOnRequestsNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests");

            migrationBuilder.AlterColumn<Guid>(
                name: "EducationOrganizationId",
                table: "Requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests",
                column: "EducationOrganizationId",
                principalTable: "EducationOrganizations",
                principalColumn: "EducationOrganizationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests");

            migrationBuilder.AlterColumn<Guid>(
                name: "EducationOrganizationId",
                table: "Requests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests",
                column: "EducationOrganizationId",
                principalTable: "EducationOrganizations",
                principalColumn: "EducationOrganizationId");
        }
    }
}
