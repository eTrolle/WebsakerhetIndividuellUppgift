using Microsoft.EntityFrameworkCore.Migrations;

namespace IndivduellUppgiftAPI.Migrations
{
    public partial class LastToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastJWT",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastRefresh",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastJWT",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastRefresh",
                table: "AspNetUsers");
        }
    }
}
