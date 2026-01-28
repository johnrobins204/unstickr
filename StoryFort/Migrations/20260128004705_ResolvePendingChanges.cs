using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class ResolvePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ArchetypePoints",
                keyColumn: "Id",
                keyValue: 14,
                column: "Label",
                value: "Development (Shō)");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "kisho",
                column: "Name",
                value: "The Twist (Kishōtenketsu)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ArchetypePoints",
                keyColumn: "Id",
                keyValue: 14,
                column: "Label",
                value: "Development (Sho)");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "kisho",
                column: "Name",
                value: "The Twist (Kishotenketsu)");
        }
    }
}
