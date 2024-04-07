using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class FixDateTimeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "InitialRequestSentDate",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "MessageTimestamp",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "InitialRequestSentDate",
                table: "Requests",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MessageTimestamp",
                table: "Messages",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
