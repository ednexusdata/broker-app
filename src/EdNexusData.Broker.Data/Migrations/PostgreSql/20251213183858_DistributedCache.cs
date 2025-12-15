using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DistributedCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CeebCode",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NcesCode",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateCode",
                table: "EducationOrganizations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DistributedCache",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<byte[]>(type: "bytea", nullable: true),
                    ExpiresAtTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SlidingExpirationInSeconds = table.Column<long>(type: "bigint", nullable: false),
                    AbsoluteExpiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedCache", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributedCache");

            migrationBuilder.DropColumn(
                name: "CeebCode",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "NcesCode",
                table: "EducationOrganizations");

            migrationBuilder.DropColumn(
                name: "StateCode",
                table: "EducationOrganizations");
        }
    }
}
