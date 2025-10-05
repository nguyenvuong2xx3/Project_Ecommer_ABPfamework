using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProducts_AppCategory_CategoryId",
                table: "AppProducts");

            migrationBuilder.DropTable(
                name: "AppTasks");

            migrationBuilder.DropTable(
                name: "AppTours");

            migrationBuilder.DropTable(
                name: "AppPersons");

            migrationBuilder.DropIndex(
                name: "IX_AppProducts_CategoryId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "State",
                table: "AppProducts");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "AppProducts",
                newName: "SKU");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppProducts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "AppProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "AppProducts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "AppProducts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "AppProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "AppProducts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "AppProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "AppCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "AppCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppCategory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppCategory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "AppCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "AppCategory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "AppCategory",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "AppCategory");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "AppCategory");

            migrationBuilder.RenameColumn(
                name: "SKU",
                table: "AppProducts",
                newName: "Image");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppProducts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "AppProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte>(
                name: "State",
                table: "AppProducts",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "AppPersons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPersons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTours",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attachment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    MaxGroupSize = table.Column<int>(type: "int", nullable: false),
                    MinGroupSize = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TourName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TourPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TourTypeCid = table.Column<long>(type: "bigint", nullable: false),
                    Transportation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedPersonId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 65536, nullable: true),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppTasks_AppPersons_AssignedPersonId",
                        column: x => x.AssignedPersonId,
                        principalTable: "AppPersons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_CategoryId",
                table: "AppProducts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppTasks_AssignedPersonId",
                table: "AppTasks",
                column: "AssignedPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppProducts_AppCategory_CategoryId",
                table: "AppProducts",
                column: "CategoryId",
                principalTable: "AppCategory",
                principalColumn: "Id");
        }
    }
}
