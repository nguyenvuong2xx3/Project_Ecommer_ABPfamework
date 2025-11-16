using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class addsalev4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductVariantId",
                table: "Sales",
                newName: "VoucherCode");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Sales",
                newName: "ProductVariantIds");

            migrationBuilder.RenameColumn(
                name: "GifCode",
                table: "Sales",
                newName: "ProductIds");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Sales",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "ApplyTo",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CategoryIds",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumDiscountAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrderValue",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsedCount",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyTo",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CategoryIds",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "MaximumDiscountAmount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "MinimumOrderValue",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UsedCount",
                table: "Sales");

            migrationBuilder.RenameColumn(
                name: "VoucherCode",
                table: "Sales",
                newName: "ProductVariantId");

            migrationBuilder.RenameColumn(
                name: "ProductVariantIds",
                table: "Sales",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "ProductIds",
                table: "Sales",
                newName: "GifCode");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Sales",
                newName: "CategoryId");
        }
    }
}
