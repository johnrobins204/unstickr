using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToAccountModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

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
                name: "Notebooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notebooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notebooks_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stories_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stories_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotebookEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    NotebookId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotebookEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotebookEntities_Notebooks_NotebookId",
                        column: x => x.NotebookId,
                        principalTable: "Notebooks",
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

            migrationBuilder.CreateTable(
                name: "NotebookEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotebookEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotebookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotebookEntries_NotebookEntities_NotebookEntityId",
                        column: x => x.NotebookEntityId,
                        principalTable: "NotebookEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryEntityLinks",
                columns: table => new
                {
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    NotebookEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryEntityLinks", x => new { x.StoryId, x.NotebookEntityId });
                    table.ForeignKey(
                        name: "FK_StoryEntityLinks_NotebookEntities_NotebookEntityId",
                        column: x => x.NotebookEntityId,
                        principalTable: "NotebookEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoryEntityLinks_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Writer" });

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
                name: "IX_NotebookEntities_NotebookId",
                table: "NotebookEntities",
                column: "NotebookId");

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntries_NotebookEntityId",
                table: "NotebookEntries",
                column: "NotebookEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Notebooks_AccountId",
                table: "Notebooks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_StoryId",
                table: "Pages",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_AccountId",
                table: "Stories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_ThemeId",
                table: "Stories",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryEntityLinks_NotebookEntityId",
                table: "StoryEntityLinks",
                column: "NotebookEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotebookEntries");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "StoryEntityLinks");

            migrationBuilder.DropTable(
                name: "NotebookEntities");

            migrationBuilder.DropTable(
                name: "Stories");

            migrationBuilder.DropTable(
                name: "Notebooks");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}

