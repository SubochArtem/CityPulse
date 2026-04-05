using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    budget_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    CityId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.id);
                    table.ForeignKey(
                        name: "FK_Polls_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Polls_Cities_CityId1",
                        column: x => x.CityId1,
                        principalTable: "Cities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Ideas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ideas", x => x.id);
                    table.ForeignKey(
                        name: "FK_Ideas_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ideas_Polls_PollId1",
                        column: x => x.PollId1,
                        principalTable: "Polls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_idea_poll_id",
                table: "Ideas",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_PollId1",
                table: "Ideas",
                column: "PollId1");

            migrationBuilder.CreateIndex(
                name: "ix_poll_city_id",
                table: "Polls",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_CityId1",
                table: "Polls",
                column: "CityId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ideas");

            migrationBuilder.DropTable(
                name: "Polls");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
