using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class fixcommentandratting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductRatings",
                newName: "ProductVariantId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductComments",
                newName: "ProductVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductVariantId",
                table: "ProductRatings",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "ProductVariantId",
                table: "ProductComments",
                newName: "ProductId");
        }
    }
}
