using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstickd.Migrations
{
    /// <inheritdoc />
    public partial class AddContentToStory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Stories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Stories");
        }
    }
}
