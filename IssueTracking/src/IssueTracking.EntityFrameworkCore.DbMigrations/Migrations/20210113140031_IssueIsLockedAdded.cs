using Microsoft.EntityFrameworkCore.Migrations;

namespace IssueTracking.Migrations
{
    public partial class IssueIsLockedAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "AppIssues",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "AppIssues");
        }
    }
}
