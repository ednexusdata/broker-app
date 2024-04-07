using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateEdOrgRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationOrganizations",
                columns: table => new
                {
                    EducationOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<string>(type: "text", nullable: true),
                    EducationOrganizationType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizations", x => x.EducationOrganizationId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizations_EducationOrganizations_ParentOrganiz~",
                        column: x => x.ParentOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRoles_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizations_ParentOrganizationId",
                table: "EducationOrganizations",
                column: "ParentOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EducationOrganizationId",
                table: "UserRoles",
                column: "EducationOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "EducationOrganizations");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
