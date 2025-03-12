using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.SimpleTaskApp.Migrations
{
    /// <inheritdoc />
    public partial class Added_ADDTASKPERION : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPersons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPersons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 65536, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    AssignedPersonId = table.Column<long>(type: "bigint", nullable: true)
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
                name: "IX_AppTasks_AssignedPersonId",
                table: "AppTasks",
                column: "AssignedPersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppTasks");

            migrationBuilder.DropTable(
                name: "AppPersons");
        }
    }
}
