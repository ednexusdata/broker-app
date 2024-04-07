using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class InitialMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalSchema = table.Column<string>(type: "jsonb", nullable: true),
                    MappingType = table.Column<string>(type: "text", nullable: true),
                    StudentAttributes = table.Column<string>(type: "jsonb", nullable: true),
                    SourceMapping = table.Column<string>(type: "jsonb", nullable: true),
                    DestinationMapping = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_Mappings_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_RequestId",
                table: "Mappings",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mappings");
        }
    }
}
