using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCityIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "Users",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Users");
        }
    }
}
