using Microsoft.EntityFrameworkCore.Migrations;

namespace Webx.Web.Migrations
{
    public partial class ProductPromotion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPromotion",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPromotion",
                table: "Products");
        }
    }
}
