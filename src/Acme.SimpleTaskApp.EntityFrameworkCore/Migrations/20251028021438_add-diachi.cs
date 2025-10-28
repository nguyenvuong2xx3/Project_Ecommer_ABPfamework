using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class adddiachi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SoDienThoai",
                table: "AbpUsers",
                newName: "IsDiaChiMacDinh");

            migrationBuilder.AddColumn<string>(
                name: "DiaChiChiTiet",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GioiTinh",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChiChiTiet",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "GioiTinh",
                table: "AbpUsers");

            migrationBuilder.RenameColumn(
                name: "IsDiaChiMacDinh",
                table: "AbpUsers",
                newName: "SoDienThoai");
        }
    }
}
