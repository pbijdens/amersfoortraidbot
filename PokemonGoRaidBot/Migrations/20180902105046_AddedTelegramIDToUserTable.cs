using Microsoft.EntityFrameworkCore.Migrations;

namespace RaidBot.Migrations
{
    public partial class AddedTelegramIDToUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleIdentity",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "TelegramUserID",
                table: "Users",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramUserID",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "GoogleIdentity",
                table: "Users",
                nullable: true);
        }
    }
}
