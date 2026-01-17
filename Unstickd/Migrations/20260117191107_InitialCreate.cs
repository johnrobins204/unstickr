using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Unstickd.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", nullable: false),
                    SecondaryColor = table.Column<string>(type: "TEXT", nullable: false),
                    FontName = table.Column<string>(type: "TEXT", nullable: false),
                    BackgroundTexture = table.Column<string>(type: "TEXT", nullable: false),
                    SpritePath = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stories_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "BackgroundTexture", "Description", "FontName", "Name", "PrimaryColor", "SecondaryColor", "SpritePath" },
                values: new object[,]
                {
                    { 1, "white", "The Queen's Rose Garden", "Alice", "Wonderland", "#eec4d5", "#ba0d2d", "images/cheshire.png" },
                    { 2, "#f0f0f0", "The Hidden Moorland Oasis", "Goudy Bookletter 1911", "Secret Garden", "#8fb35d", "#5d8aa8", "images/robin.png" },
                    { 3, "#0d1b2a", "Neverland at Twilight", "Cinzel Decorative", "Neverland", "#191970", "#ffd700", "images/pan.png" },
                    { 4, "#fafff0", "The Emerald City", "Rye", "Oz", "#50c878", "#daa520", "images/toto.png" },
                    { 5, "#f5deb3", "Sun-Bleached Map", "Pirata One", "Treasure Island", "#d2b48c", "#000080", "images/parrot.png" },
                    { 6, "#fdf5e6", "Wind in the Willows", "WindSong", "Riverbank", "#8b4513", "#9dc183", "images/mole.png" },
                    { 7, "#fff0f5", "The Velveteen Rabbit", "Sniglet", "Nursery", "#f5f5dc", "#b76e79", "images/rabbit.png" },
                    { 8, "#fff8dc", "Geppetto's Workshop", "Geostar Fill", "Workshop", "#deb887", "#6495ed", "images/cricket.png" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_StoryId",
                table: "Pages",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_ThemeId",
                table: "Stories",
                column: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Stories");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
