using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitIdeaStatusToAccessStatusAndApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "access_status",
                table: "Ideas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "approval_status",
                table: "Ideas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql(@"
                UPDATE ""Ideas""
                SET 
                    access_status = CASE status
                        WHEN 1 THEN 1  
                        WHEN 5 THEN 2 
                        WHEN 6 THEN 2  
                        ELSE 0        
                    END,
                    approval_status = CASE status
                        WHEN 1 THEN 4  
                        WHEN 5 THEN 4 
                        WHEN 6 THEN 4  
                        ELSE 0      
                    END;
            ");
            
            migrationBuilder.DropColumn(
                name: "status",
                table: "Ideas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "Ideas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""Ideas""
                SET status = CASE
                    WHEN access_status = 1 AND approval_status = 4 THEN 1 
                    WHEN access_status = 2 AND approval_status = 4 THEN 6 
                    ELSE 0
                END;
            ");

            migrationBuilder.DropColumn(
                name: "access_status",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "approval_status",
                table: "Ideas");
        }
    }
}
