using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UniqueKeyOnUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_EducationOrganizationId",
                table: "UserRoles");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Users",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EducationOrganizationId_UserId",
                table: "UserRoles",
                columns: new[] { "EducationOrganizationId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_EducationOrganizationId_UserId",
                table: "UserRoles");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Users",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EducationOrganizationId",
                table: "UserRoles",
                column: "EducationOrganizationId");
        }
    }
}
