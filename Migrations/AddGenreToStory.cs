using Microsoft.EntityFrameworkCore.Migrations;

namespace Unstickd.Migrations;

public partial class AddGenreToStory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Genre",
            table: "Stories",
            type: "TEXT",
            nullable: false,
            defaultValue: "General");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Genre",
            table: "Stories");
    }
}
