using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class AddArchetypeExamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchetypeExamples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArchetypePointId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchetypeExamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchetypeExamples_ArchetypePoints_ArchetypePointId",
                        column: x => x.ArchetypePointId,
                        principalTable: "ArchetypePoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ArchetypeExamples",
                columns: new[] { "Id", "ArchetypePointId", "Content", "Title" },
                values: new object[,]
                {
                    { 1, 7, "Dorothy lives on a gray, dry prairie in Kansas with Aunt Em and Uncle Henry. Her life is dull, and she dreams of 'Somewhere Over the Rainbow'.", "The Wizard of Oz" },
                    { 2, 8, "The mean Miss Gulch takes Toto away with a sheriff's order to be destroyed. Toto escapes, but Dorothy decides she must run away to keep him safe.", "The Wizard of Oz" },
                    { 3, 9, "A cyclone rips the farmhouse from the ground and deposits it in the colorful Land of Oz. Dorothy opens the door to a technicolor world, leaving the black-and-white Kansas behind.", "The Wizard of Oz" },
                    { 4, 10, "Dorothy and her friends are captured by Flying Monkeys and trapped in the Wicked Witch's castle. The hour glass is running out!", "The Wizard of Oz" },
                    { 5, 11, "The Wizard offers to take Dorothy home in his balloon, but it accidentally launches while she is chasing Toto, leaving her stranded again.", "The Wizard of Oz" },
                    { 6, 12, "Dorothy learns she had the power all along. She taps her ruby slippers three times, says 'There's no place like home', and wakes up in her own bed surrounded by her family.", "The Wizard of Oz" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypeExamples_ArchetypePointId",
                table: "ArchetypeExamples",
                column: "ArchetypePointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchetypeExamples");
        }
    }
}

