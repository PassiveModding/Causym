using Microsoft.EntityFrameworkCore.Migrations;

namespace Causym.Migrations
{
    public partial class CommandFailureOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RespondOnCommandFailure",
                table: "Guilds",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RespondOnCommandFailure",
                table: "Guilds");
        }
    }
}