using Microsoft.EntityFrameworkCore.Migrations;

namespace IndivduellUppgiftAPI.Migrations
{
    public partial class jti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "jti",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "jti",
                table: "AspNetUsers");
        }
    }
}
