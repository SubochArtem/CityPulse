using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccessStatusToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessStatus",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessStatus",
                table: "Users");
        }
    }
}
