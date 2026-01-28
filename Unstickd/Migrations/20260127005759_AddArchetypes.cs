using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Unstickd.Migrations
{
    /// <inheritdoc />
    public partial class AddArchetypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archetypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SvgPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archetypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchetypePoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArchetypeId = table.Column<string>(type: "TEXT", nullable: false),
                    StepId = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Align = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchetypePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchetypePoints_Archetypes_ArchetypeId",
                        column: x => x.ArchetypeId,
                        principalTable: "Archetypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Archetypes",
                columns: new[] { "Id", "Description", "Name", "SvgPath" },
                values: new object[,]
                {
                    { "classic", "A classic rise and fall structure suitable for most stories.", "Classic Arc (Freytag)", "M0,320 L100,320 L300,50 L500,320 L800,320" },
                    { "hero", "A circular journey where the hero leaves home and returns changed.", "Hero's Journey", "M0,320 L100,320 L250,150 L400,38 L550,150 L700,320 L800,320" },
                    { "kisho", "An East Asian structure with four acts: Intro, Development, Twist, and Conclusion.", "The Twist (Kishōtenketsu)", "M0,320 L100,320 L300,200 L550,50 L750,320 L800,320" }
                });

            migrationBuilder.InsertData(
                table: "ArchetypePoints",
                columns: new[] { "Id", "Align", "ArchetypeId", "Label", "Prompt", "StepId", "X", "Y" },
                values: new object[,]
                {
                    { 1, "center", "classic", "Beginning", "How does the story start? Describe the normal world.", 1, 50.0, 308.0 },
                    { 2, "center", "classic", "Inciting Incident", "What event changes everything for the hero?", 2, 150.0, 250.0 },
                    { 3, "right", "classic", "Rising Action", "What obstacles does the hero face along the way?", 3, 225.0, 150.0 },
                    { 4, "center", "classic", "The Climax", "The biggest battle or challenge!", 4, 300.0, 38.0 },
                    { 5, "left", "classic", "Falling Action", "What happens immediately after the climax?", 5, 400.0, 185.0 },
                    { 6, "center", "classic", "Resolution", "How does the story end? What is the new normal?", 6, 650.0, 308.0 },
                    { 7, "center", "hero", "Ordinary World", "Describe the hero's life before the adventure.", 1, 50.0, 308.0 },
                    { 8, "right", "hero", "Call to Adventure", "Who or what calls them to action?", 2, 175.0, 235.0 },
                    { 9, "right", "hero", "Threshold", "The hero leaves home and enters the unknown.", 3, 250.0, 150.0 },
                    { 10, "center", "hero", "The Ordeal", "The central crisis where the hero faces their greatest fear.", 4, 400.0, 38.0 },
                    { 11, "left", "hero", "The Road Back", "The hero must return home with what they learned.", 5, 550.0, 150.0 },
                    { 12, "center", "hero", "Return w/ Elixir", "The hero returns home, changed forever.", 6, 700.0, 308.0 },
                    { 13, "center", "kisho", "Introduction (Ki)", "Introduce the characters and their world.", 1, 50.0, 308.0 },
                    { 14, "center", "kisho", "Development (Shō)", "Deepen the story. What are they doing? (No major conflict yet)", 2, 200.0, 250.0 },
                    { 15, "center", "kisho", "The Twist (Ten)", "Surprise! Something unexpected happens that changes everything.", 3, 550.0, 50.0 },
                    { 16, "center", "kisho", "Conclusion (Ketsu)", "How does the story settle after the twist?", 4, 750.0, 308.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypePoints_ArchetypeId",
                table: "ArchetypePoints",
                column: "ArchetypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchetypePoints");

            migrationBuilder.DropTable(
                name: "Archetypes");
        }
    }
}
