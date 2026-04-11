using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPollScheduleJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "poll_schedule_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    poll_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hangfire_job_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_schedule_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_poll_schedule_jobs_Polls_poll_id",
                        column: x => x.poll_id,
                        principalTable: "Polls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_poll_schedule_job_poll_id",
                table: "poll_schedule_jobs",
                column: "poll_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "poll_schedule_jobs");
        }
    }
}
