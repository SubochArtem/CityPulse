using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImagesAndDeletedImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "city_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_city_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_city_images_Cities_city_id",
                        column: x => x.city_id,
                        principalTable: "Cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deleted_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    queued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deleted_images", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idea_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idea_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_idea_images_Ideas_idea_id",
                        column: x => x.idea_id,
                        principalTable: "Ideas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    poll_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_poll_images_Polls_poll_id",
                        column: x => x.poll_id,
                        principalTable: "Polls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_city_images_city_id",
                table: "city_images",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_idea_images_idea_id",
                table: "idea_images",
                column: "idea_id");

            migrationBuilder.CreateIndex(
                name: "ix_poll_images_poll_id",
                table: "poll_images",
                column: "poll_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "city_images");

            migrationBuilder.DropTable(
                name: "deleted_images");

            migrationBuilder.DropTable(
                name: "idea_images");

            migrationBuilder.DropTable(
                name: "poll_images");
        }
    }
}
