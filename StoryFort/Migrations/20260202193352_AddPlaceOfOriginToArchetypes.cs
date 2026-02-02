using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceOfOriginToArchetypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceOfOrigin",
                table: "Archetypes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "classic",
                column: "PlaceOfOrigin",
                value: "Germany");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "dilemma",
                column: "PlaceOfOrigin",
                value: "West Africa");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "exile_restore",
                column: "PlaceOfOrigin",
                value: "India");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "fable",
                column: "PlaceOfOrigin",
                value: "Greece");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "hero",
                column: "PlaceOfOrigin",
                value: "Universal");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "iterative",
                column: "PlaceOfOrigin",
                value: "Russia");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "kisho",
                column: "PlaceOfOrigin",
                value: "East Asia");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "light_bringer",
                column: "PlaceOfOrigin",
                value: "Pacific Northwest");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "monster",
                column: "PlaceOfOrigin",
                value: "Universal");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "ordeal",
                column: "PlaceOfOrigin",
                value: "Russia");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "pilgrimage",
                column: "PlaceOfOrigin",
                value: "India");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "quest",
                column: "PlaceOfOrigin",
                value: "Universal");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "rags_riches",
                column: "PlaceOfOrigin",
                value: "Universal");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "resistance",
                column: "PlaceOfOrigin",
                value: "China");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "sentinel",
                column: "PlaceOfOrigin",
                value: "Russia");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "transformation",
                column: "PlaceOfOrigin",
                value: "Greece");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "trickster",
                column: "PlaceOfOrigin",
                value: "West Africa");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "voyage",
                column: "PlaceOfOrigin",
                value: "Universal");

            migrationBuilder.UpdateData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValue: "wyrd",
                column: "PlaceOfOrigin",
                value: "Scandinavia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceOfOrigin",
                table: "Archetypes");
        }
    }
}
