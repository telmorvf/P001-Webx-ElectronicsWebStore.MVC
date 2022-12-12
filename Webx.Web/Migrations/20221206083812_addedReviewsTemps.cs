using Microsoft.EntityFrameworkCore.Migrations;

namespace Webx.Web.Migrations
{
    public partial class addedReviewsTemps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewsTemps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductReviewId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewsTemps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewsTemps_Reviews_ProductReviewId",
                        column: x => x.ProductReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewsTemps_ProductReviewId",
                table: "ReviewsTemps",
                column: "ProductReviewId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewsTemps");
        }
    }
}
