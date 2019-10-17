using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VkBot.Migrations
{
    public partial class GeoPos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeoPositions",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Place = table.Column<string>(nullable: true),
                    Commentary = table.Column<string>(nullable: true),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoPositions", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeoPositions");
        }
    }
}
