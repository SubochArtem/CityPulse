using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeUpdatedAtNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Polls",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Ideas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Cities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.Sql(
                "UPDATE \"Polls\" SET updated_at = NULL WHERE updated_at = created_at;");
            migrationBuilder.Sql(
                "UPDATE \"Ideas\" SET updated_at = NULL WHERE updated_at = created_at;");
            migrationBuilder.Sql(
                "UPDATE \"Cities\" SET updated_at = NULL WHERE updated_at = created_at;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Polls\" SET updated_at = created_at WHERE updated_at IS NULL;");
            migrationBuilder.Sql(
                "UPDATE \"Ideas\" SET updated_at = created_at WHERE updated_at IS NULL;");
            migrationBuilder.Sql(
                "UPDATE \"Cities\" SET updated_at = created_at WHERE updated_at IS NULL;");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Polls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Ideas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Cities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
