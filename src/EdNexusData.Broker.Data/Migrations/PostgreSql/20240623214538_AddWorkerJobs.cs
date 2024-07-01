using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddWorkerJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Worker_Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    JobType = table.Column<string>(type: "text", nullable: true),
                    JobParameters = table.Column<string>(type: "jsonb", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    ReferenceGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    JobStatus = table.Column<int>(type: "integer", nullable: false),
                    WorkerInstance = table.Column<string>(type: "text", nullable: true),
                    WorkerState = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worker_Jobs", x => x.JobId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Worker_Jobs");
        }
    }
}
