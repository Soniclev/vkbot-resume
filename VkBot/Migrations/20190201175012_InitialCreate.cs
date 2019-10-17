using Microsoft.EntityFrameworkCore.Migrations;

namespace VkBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nicknames",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nick = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nicknames", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nicknames");
        }
    }
}
