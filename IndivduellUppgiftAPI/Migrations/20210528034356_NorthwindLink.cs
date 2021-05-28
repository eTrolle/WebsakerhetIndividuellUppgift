using Microsoft.EntityFrameworkCore.Migrations;

namespace IndivduellUppgiftAPI.Migrations
{
    public partial class NorthwindLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NorthwindLink",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NorthwindLink",
                table: "AspNetUsers");
        }
    }
}
