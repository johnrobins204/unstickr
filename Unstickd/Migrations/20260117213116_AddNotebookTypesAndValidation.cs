using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Unstickd.Migrations
{
    /// <inheritdoc />
    public partial class AddNotebookTypesAndValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoryId",
                table: "NotebookEntries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotebookTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotebookTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NotebookTypes",
                columns: new[] { "Id", "Icon", "IsSystemDefault", "Name" },
                values: new object[,]
                {
                    { 1, "bi-person", true, "Characters" },
                    { 2, "bi-geo-alt", true, "Places" },
                    { 3, "bi-magic", true, "Spells" },
                    { 4, "bi-egg-fried", true, "Recipes" },
                    { 5, "bi-bug", true, "Creatures" },
                    { 6, "bi-box-seam", true, "Items" },
                    { 7, "bi-book", true, "Lore" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntries_StoryId",
                table: "NotebookEntries",
                column: "StoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotebookEntries_Stories_StoryId",
                table: "NotebookEntries",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotebookEntries_Stories_StoryId",
                table: "NotebookEntries");

            migrationBuilder.DropTable(
                name: "NotebookTypes");

            migrationBuilder.DropIndex(
                name: "IX_NotebookEntries_StoryId",
                table: "NotebookEntries");

            migrationBuilder.DropColumn(
                name: "StoryId",
                table: "NotebookEntries");
        }
    }
}
