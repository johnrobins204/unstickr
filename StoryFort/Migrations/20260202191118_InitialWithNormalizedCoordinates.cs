using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithNormalizedCoordinates : Migration
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

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SupervisorName = table.Column<string>(type: "TEXT", nullable: false),
                    SupervisorEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ProtectedCohereApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    UseReasoningModel = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActiveThemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    ThemePreferenceJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Themes_ActiveThemeId",
                        column: x => x.ActiveThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateTable(
                name: "AccountApiKeyHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProtectedKey = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountApiKeyHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountApiKeyHistories_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notebooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Genre = table.Column<string>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "NotebookEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    NotebookId = table.Column<int>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "NotebookEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotebookEntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_NotebookEntries_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id");
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
                columns: new[] { "Id", "ActiveThemeId", "Name", "ProtectedCohereApiKey", "SupervisorEmail", "SupervisorName", "ThemePreferenceJson", "UseReasoningModel" },
                values: new object[] { 1, null, "Writer", "", "", "", "{}", true });

            migrationBuilder.InsertData(
                table: "Archetypes",
                columns: new[] { "Id", "Description", "Name", "SvgPath" },
                values: new object[,]
                {
                    { "classic", "A five-part dramatic structure: Exposition, Rising Action, Climax, Falling Action, and Resolution.", "Freytag's Pyramid", "M 0,0.9 L 0.5,0.1 L 1,0.9" },
                    { "dilemma", "A narrative structure common in African oral tradition that presents a difficult moral or ethical choice, ending with a question for the audience.", "The Dilemma Tale", "M 0,0.8 L 0.5,0.2 M 0.5,0.2 L 1,0.1 M 0.5,0.2 L 1,0.4" },
                    { "exile_restore", "A high-status protagonist is unjustly cast out into the wilderness, endures a long period of hardship and loss, and eventually returns to their rightful place.", "Exile and Restoration", "M 0,0.2 L 0.2,0.2 C 0.4,0.9 0.6,0.9 0.8,0.2 L 1,0.2" },
                    { "fable", "A concise narrative, often featuring animals, that leads to a clear ethical or tactical lesson through the use of wit.", "The Moral Fable", "M 0,0.6 L 0.7,0.6 L 0.8,0.2 L 1,0.2" },
                    { "hero", "The common template of stories that involve a hero who goes on an adventure, wins a victory in a decisive crisis, and comes home changed.", "The Hero's Journey", "M 0,0.8 C 0.2,0.8 0.3,0.5 0.5,0.2 C 0.7,0.5 0.8,0.8 1,0.8" },
                    { "iterative", "The protagonist must complete a series of escalating tasks, usually under the watch of a dangerous supernatural figure, to earn their freedom or a boon.", "The Iterative Trial", "M 0,0.9 L 0.3,0.9 L 0.3,0.6 L 0.6,0.6 L 0.6,0.3 L 1,0.3" },
                    { "kisho", "A four-act structure common in East Asian narratives. It focuses on a 'twist' or change in perspective rather than a central conflict.", "Kishōtenketsu", "M 0,0.7 L 0.33,0.7 L 0.66,0.2 L 1,0.2" },
                    { "light_bringer", "A structure where a trickster figure infiltrates a restricted domain to retrieve a vital element (light, fire, water) for the world's benefit.", "The Bringer of Light", "M 0,0.8 Q 0.3,0.9 0.5,0.8 Q 0.7,0.8 0.8,0.2 L 1,0.1" },
                    { "monster", "The hero sets out to destroy an evil force which threatens the land. Usually involves a perilous climb and a narrow escape.", "Overcoming the Monster", "M 0,0.9 L 0.4,0.6 L 0.5,0.1 L 0.6,0.6 L 1,0.9" },
                    { "ordeal", "An escalating narrative structure where the hero must face three major, distinct conflicts, often separated by long periods of time.", "The Triple Ordeal", "M 0,0.8 L 0.2,0.5 L 0.4,0.8 L 0.6,0.3 L 0.8,0.8 L 1,0.1" },
                    { "pilgrimage", "A structure where the protagonist undergoes a long journey involving repeated trials that test and build their spiritual or moral character.", "The Pilgrimage Arc", "M 0,0.9 Q 0.1,0.7 0.2,0.9 Q 0.3,0.6 0.4,0.8 Q 0.5,0.4 0.6,0.7 Q 0.8,0.1 1,0.2" },
                    { "quest", "A hero and their companions set out to find a specific person or object, overcoming a series of trials until the goal is achieved.", "The Quest", "M 0,0.8 L 0.2,0.8 L 0.4,0.6 L 0.6,0.6 L 0.8,0.3 L 1,0.1" },
                    { "rags_riches", "A protagonist from humble beginnings rises to great heights, suffers a temporary loss or crisis, and finally achieves permanent success.", "Rags to Riches", "M 0,0.9 L 0.3,0.7 L 0.5,0.2 L 0.7,0.6 L 1,0.1" },
                    { "resistance", "The story of a protagonist who remains steadfast in their values despite intense suffering and external pressure, leading to eventual vindication.", "The Virtuous Resistance", "M 0,0.4 L 0.2,0.4 C 0.3,0.4 0.4,0.9 0.6,0.9 L 0.8,0.9 L 1,0.1" },
                    { "sentinel", "A structure featuring a long period of preparation or stasis, followed by a sudden call to action and a journey to protect the realm.", "The Heroic Sentinel", "M 0,0.9 L 0.4,0.9 L 0.4,0.5 L 1,0.1" },
                    { "transformation", "A narrative of fundamental change where a protagonist descends into a new element or state of being, establishing a new order of nature.", "The Deep Transformation", "M 0,0.2 L 0.4,0.2 L 0.5,1.0 L 1,1.0" },
                    { "trickster", "A narrative focused on a clever protagonist who uses wit and deception to solve problems or gain an advantage, often disrupting the status quo.", "The Trickster's Cycle", "M 0,0.8 L 0.2,0.3 L 0.4,0.8 L 0.6,0.3 L 0.8,0.8 L 1,0.5" },
                    { "voyage", "The protagonist travels to a strange world and, after overcoming the threats it poses or learning important lessons, returns with experience.", "Voyage and Return", "M 0,0.5 Q 0.5,-0.2 1,0.5" },
                    { "wyrd", "A structure common in Old Norse literature where the protagonist's doom is preordained, and the story focuses on their dignified path toward that end.", "The Tragic Fate", "M 0,0.2 L 1,0.9" }
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

            migrationBuilder.InsertData(
                table: "ArchetypePoints",
                columns: new[] { "Id", "Align", "ArchetypeId", "Label", "Prompt", "StepId", "X", "Y" },
                values: new object[,]
                {
                    { 1, "left", "hero", "Ordinary World", "Describe your protagonist's daily life before the adventure begins.", 1, 0.0, 0.80000000000000004 },
                    { 2, "top", "hero", "Call to Adventure", "What event disrupts the hero's world and presents a challenge?", 2, 0.14999999999999999, 0.69999999999999996 },
                    { 3, "top", "hero", "Crossing the Threshold", "The hero leaves the known world. What is the point of no return?", 3, 0.34999999999999998, 0.45000000000000001 },
                    { 4, "center", "hero", "The Ordeal", "The greatest challenge. What is the 'lowest point' for your hero?", 4, 0.5, 0.10000000000000001 },
                    { 5, "right", "hero", "Return with Elixir", "How is the hero changed? What do they bring back to their world?", 5, 1.0, 0.80000000000000004 },
                    { 6, "left", "voyage", "Falling into the Unknown", "How does the character enter the strange new world?", 1, 0.050000000000000003, 0.5 },
                    { 7, "top", "voyage", "The Strange World", "Describe the initial wonders and confusing rules of this new place.", 2, 0.29999999999999999, 0.20000000000000001 },
                    { 8, "bottom", "voyage", "The Great Escape", "A threat arises. How does the hero find their way back home?", 3, 0.80000000000000004, 0.29999999999999999 },
                    { 9, "left", "kisho", "Ki (Introduction)", "Introduce the characters and their peaceful environment.", 1, 0.0, 0.69999999999999996 },
                    { 10, "bottom", "kisho", "Shō (Development)", "Expand on the world. What are the characters doing day-to-day?", 2, 0.33000000000000002, 0.69999999999999996 },
                    { 11, "top", "kisho", "Ten (Twist)", "Introduce a surprise or a change in perspective unrelated to the main plot.", 3, 0.66000000000000003, 0.20000000000000001 },
                    { 12, "right", "kisho", "Ketsu (Conclusion)", "How does the twist resolve the story in a new, unified way?", 4, 1.0, 0.20000000000000001 },
                    { 13, "left", "monster", "The Threat", "Identify the monster and the danger it poses to the community.", 1, 0.10000000000000001, 0.90000000000000002 },
                    { 14, "top", "monster", "The Ascent", "The hero approaches the monster's lair. What obstacles do they face?", 2, 0.40000000000000002, 0.5 },
                    { 15, "center", "monster", "The Final Battle", "The hero confronts the monster. This is the peak of the action.", 3, 0.5, 0.10000000000000001 },
                    { 16, "left", "trickster", "The Ambition", "What does the trickster want, and what clever plan do they devise?", 1, 0.0, 0.80000000000000004 },
                    { 17, "top", "trickster", "The Deception", "Describe the trick played on the unsuspecting target.", 2, 0.20000000000000001, 0.29999999999999999 },
                    { 18, "bottom", "trickster", "The Complication", "What goes wrong or makes the scheme nearly fail?", 3, 0.40000000000000002, 0.80000000000000004 },
                    { 19, "right", "trickster", "The Narrow Escape", "How does the trickster get away with the prize (or just their life)?", 4, 1.0, 0.5 },
                    { 20, "left", "dilemma", "The Scenario", "Introduce the characters and the conflict that leads to the choice.", 1, 0.0, 0.80000000000000004 },
                    { 21, "center", "dilemma", "The Choice", "What are the conflicting moral paths or difficult decisions presented?", 2, 0.5, 0.20000000000000001 },
                    { 22, "right", "dilemma", "The Unresolved End", "Leave the story open. Ask the audience: What should happen next?", 3, 1.0, 0.25 },
                    { 23, "left", "pilgrimage", "Spiritual Call", "What higher purpose or mission motivates the journey?", 1, 0.0, 0.90000000000000002 },
                    { 24, "top", "pilgrimage", "Cyclical Trials", "Describe a recurring challenge that tests the hero's resolve.", 2, 0.29999999999999999, 0.59999999999999998 },
                    { 25, "right", "pilgrimage", "Enlightenment", "The journey reaches its end. What profound truth is attained?", 3, 1.0, 0.20000000000000001 },
                    { 26, "left", "resistance", "Unbreakable Bond", "What promise or relationship defines the hero's virtue?", 1, 0.0, 0.40000000000000002 },
                    { 27, "bottom", "resistance", "Deepest Ordeal", "The hero is imprisoned or pressured to betray their values. Describe their suffering.", 2, 0.59999999999999998, 0.90000000000000002 },
                    { 28, "right", "resistance", "Justice & Reunion", "The truth is revealed. How is the hero vindicated?", 3, 1.0, 0.10000000000000001 },
                    { 29, "left", "wyrd", "The Prophecy", "What knowledge of their inevitable end does the hero possess?", 1, 0.0, 0.20000000000000001 },
                    { 30, "center", "wyrd", "The Choice of Honor", "Knowing the end is near, what action does the hero take to ensure their legacy?", 2, 0.5, 0.55000000000000004 },
                    { 31, "right", "wyrd", "Final Doom", "The fate is met. Describe the hero's final stand.", 3, 1.0, 0.90000000000000002 },
                    { 32, "top", "ordeal", "First Great Deed", "The hero's first major challenge. What proves their worth?", 1, 0.20000000000000001, 0.5 },
                    { 33, "top", "ordeal", "The Shadow Trial", "A more dangerous, hidden foe emerges. How do they survive?", 2, 0.59999999999999998, 0.29999999999999999 },
                    { 34, "right", "ordeal", "The Last Battle", "Decades later, a final threat arrives. What is the hero's ultimate sacrifice?", 3, 1.0, 0.10000000000000001 },
                    { 35, "left", "fable", "The Clever Setup", "Introduce the characters and a tactical problem they must solve.", 1, 0.0, 0.59999999999999998 },
                    { 36, "bottom", "fable", "The Turning Point", "A clever trick or realization occurs. How does the character outsmart the problem?", 2, 0.69999999999999996, 0.59999999999999998 },
                    { 37, "right", "fable", "The Moral", "State the lesson learned. How should this change the reader's behavior?", 3, 1.0, 0.20000000000000001 },
                    { 38, "left", "exile_restore", "The Fall from Grace", "What injustice leads the protagonist to lose their home and status?", 1, 0.0, 0.20000000000000001 },
                    { 39, "bottom", "exile_restore", "The Forest Years", "Describe the long period of wandering and survival in the wilderness.", 2, 0.5, 0.90000000000000002 },
                    { 40, "right", "exile_restore", "The Rightful Return", "The protagonist reclaims their place. How is order finally restored?", 3, 1.0, 0.20000000000000001 },
                    { 41, "left", "sentinel", "The Long Stasis", "Describe the hero's period of inactivity. Why are they unable or unwilling to act?", 1, 0.0, 0.90000000000000002 },
                    { 42, "top", "sentinel", "The Sudden Strength", "What supernatural or external force grants the hero the power to move?", 2, 0.40000000000000002, 0.5 },
                    { 43, "right", "sentinel", "The Vigil", "The hero stands guard at a critical location. Who or what do they defend?", 3, 1.0, 0.10000000000000001 },
                    { 44, "bottom", "iterative", "The First Task", "The introductory test. What simple but crucial instruction is given?", 1, 0.29999999999999999, 0.90000000000000002 },
                    { 45, "center", "iterative", "The Midnight Test", "A more difficult task requiring magical aid. What happens when the world is asleep?", 2, 0.59999999999999998, 0.59999999999999998 },
                    { 46, "right", "iterative", "The Final Freedom", "The hardest task. How does the hero escape the master's power?", 3, 1.0, 0.29999999999999999 },
                    { 47, "left", "light_bringer", "The Infiltration", "How does the trickster sneak into the place where the light is hidden?", 1, 0.0, 0.80000000000000004 },
                    { 48, "bottom", "light_bringer", "The Hidden Treasure", "Describe the moment the trickster finds the sun, moon, or fire.", 2, 0.5, 0.80000000000000004 },
                    { 49, "right", "light_bringer", "The Flight of Light", "The trickster escapes and releases the light into the world. What is the impact?", 3, 1.0, 0.10000000000000001 },
                    { 50, "left", "transformation", "The Sacrifice", "What tragic event or betrayal forces the protagonist into a new realm?", 1, 0.0, 0.20000000000000001 },
                    { 51, "bottom", "transformation", "The Deep Change", "Describe the physical or spiritual transformation as they descend.", 2, 0.5, 1.0 },
                    { 52, "right", "transformation", "The New Order", "The protagonist now rules a new domain. How does this provide for the world?", 3, 1.0, 1.0 },
                    { 53, "left", "rags_riches", "Initial Misery", "Describe the protagonist's humble or oppressed starting condition.", 1, 0.0, 0.90000000000000002 },
                    { 54, "top", "rags_riches", "The False Peak", "The character achieves a sudden, temporary success. What magic or luck makes this happen?", 2, 0.5, 0.20000000000000001 },
                    { 55, "bottom", "rags_riches", "The Midnight Crisis", "The success is lost or threatened. What is the turning point?", 3, 0.69999999999999996, 0.59999999999999998 },
                    { 56, "right", "rags_riches", "Permanent Rise", "The character is vindicated. How is their success made permanent and authentic?", 4, 1.0, 0.10000000000000001 },
                    { 57, "left", "quest", "The Call & Mission", "What specific object or person must the hero retrieve, and why?", 1, 0.0, 0.80000000000000004 },
                    { 58, "top", "quest", "The First Guardian", "The first major obstacle blocking the path. How do they overcome it?", 2, 0.40000000000000002, 0.59999999999999998 },
                    { 59, "top", "quest", "The Final Trial", "The most dangerous test right before the goal. What is sacrificed?", 3, 0.80000000000000004, 0.29999999999999999 },
                    { 60, "right", "quest", "The Retrieval", "The goal is achieved. Describe the moment the prize is secured.", 4, 1.0, 0.10000000000000001 }
                });

            migrationBuilder.InsertData(
                table: "ArchetypeExamples",
                columns: new[] { "Id", "ArchetypePointId", "Content", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Dorothy lives in a gray, dry Kansas with Aunt Em and Uncle Henry, feeling bored and out of place.", "The Wonderful Wizard of Oz" },
                    { 2, 2, "A cyclone arrives, lifting the house with Dorothy and Toto inside into the sky.", "The Wonderful Wizard of Oz" },
                    { 3, 4, "Dorothy is captured by the Wicked Witch of the West and must find a way to destroy her to save her friends.", "The Wonderful Wizard of Oz" },
                    { 4, 6, "Alice follows the White Rabbit and falls down a deep rabbit hole into a bizarre world.", "Alice in Wonderland" },
                    { 5, 8, "The Queen of Hearts demands Alice's execution. Alice realizes the creatures are 'nothing but a pack of cards' and wakes up.", "Alice in Wonderland" },
                    { 6, 9, "An old bamboo cutter finds a tiny, glowing baby girl inside a bamboo stalk and raises her as Princess Kaguya.", "The Tale of the Bamboo Cutter" },
                    { 7, 11, "Despite many suitors and great wealth, Kaguya reveals she is not from this world but from the Moon, and her people are coming to take her back.", "The Tale of the Bamboo Cutter" },
                    { 8, 13, "A giant lives at the top of a massive beanstalk and has stolen the wealth of Jack's family.", "Jack and the Beanstalk" },
                    { 9, 15, "Jack steals the Giant's golden-egg-laying hen and chops down the beanstalk as the Giant chases him, defeating the monster.", "Jack and the Beanstalk" },
                    { 10, 16, "Anansi the Spider wants to buy the stories owned by Nyame the Sky God, but the price is high.", "Anansi and the Box of Stories" },
                    { 11, 17, "Anansi tricks a hornets' nest, a python, and a leopard into capture using only his wits and some simple tools.", "Anansi and the Box of Stories" },
                    { 12, 20, "A King offers his daughter's hand to whoever can perform an impossible task. Three suitors succeed in different ways.", "The King's Daughter and the Suitors" },
                    { 13, 22, "The story ends by asking: 'Which of the three suitors truly deserves the reward?'", "The King's Daughter and the Suitors" },
                    { 14, 23, "The monk Xuanzang is tasked by the Tang Emperor and the Bodhisattva Guanyin to travel to India to retrieve sacred Buddhist scriptures.", "Journey to the West" },
                    { 15, 24, "Sun Wukong (The Monkey King) and the other disciples protect the monk from 81 tribulations, including demons who want to eat his flesh.", "Journey to the West" },
                    { 16, 26, "Chunhyang and Mongryong fall deeply in love and secretly marry, promising eternal fidelity before Mongryong must leave for the capital.", "The Story of Chunhyang" },
                    { 17, 27, "The corrupt official Byeon demands Chunhyang become his mistress. She refuses and is brutally tortured and imprisoned, facing death for her virtue.", "The Story of Chunhyang" },
                    { 18, 29, "Sigurd is born into a line of heroes but is warned by prophecies and his uncle that he is destined for a life of great glory followed by an early, tragic death.", "The Saga of the Volsungs" },
                    { 19, 31, "Despite knowing the betrayal is coming, Sigurd maintains his honor. He is killed in his bed, fulfilling the 'Wyrd' of the Volsung line.", "The Saga of the Volsungs" },
                    { 20, 32, "As a young warrior, Beowulf travels to Denmark to defeat the monster Grendel, who has been terrorizing King Hrothgar's hall, Heorot.", "Beowulf" },
                    { 21, 34, "Fifty years after his first battles, King Beowulf must face a final, massive dragon. He slays the beast but sustains a mortal wound, dying to save his kingdom.", "Beowulf" },
                    { 22, 35, "A hungry lion named Bhasuraka is terrorizing the forest. To stop the slaughter, the animals agree to send one animal each day to be his meal.", "The Lion and the Hare" },
                    { 23, 37, "A tiny hare leads the lion to a deep well, claiming another lion lives inside. The lion jumps in to fight his reflection and drowns. Moral: Wit is superior to brute force.", "The Lion and the Hare" },
                    { 24, 38, "Prince Rama is about to be crowned King, but his stepmother Kaikeyi demands he be exiled to the Dandaka forest for 14 years so her own son can rule.", "The Ramayana" },
                    { 25, 40, "After 14 years and defeating the demon king Ravana, Rama returns to Ayodhya. The citizens light lamps to celebrate his return, and he is finally crowned King.", "The Ramayana" },
                    { 26, 41, "Ilya sits on the stove for thirty-three years, unable to move his hands or feet, until two passing pilgrims heal him and give him heroic strength.", "Ilya Muromets and Nightingale the Robber" },
                    { 27, 43, "Ilya journeys to Kiev, clearing the road of the monster Nightingale the Robber and standing guard at the city gates as the premier bogatyr (sentinel).", "Ilya Muromets and Nightingale the Robber" },
                    { 28, 44, "Vasilisa is sent by her cruel stepmother into the woods to fetch light from the hut of the witch Baba Yaga.", "Vasilisa the Beautiful" },
                    { 29, 45, "Baba Yaga forces Vasilisa to sort massive piles of wheat and poppy seeds by night. Vasilisa succeeds each time with the help of her magical doll.", "Vasilisa the Beautiful" },
                    { 30, 46, "After Vasilisa answers Baba Yaga's questions about her virtue, the witch gives her a skull lantern and allows her to leave, freeing her from service.", "Vasilisa the Beautiful" },
                    { 31, 47, "The world is in total darkness because an old man keeps the light in a box. Raven turns himself into a pine needle, is swallowed by the man's daughter, and is born as a baby boy.", "Raven Steals the Light" },
                    { 32, 49, "As a toddler, Raven cries for the boxes. He opens them one by one, releasing the stars and moon. Finally, he grabs the sun, turns back into a bird, and flies through the smoke hole, placing the sun in the sky.", "Raven Steals the Light" },
                    { 33, 50, "Sedna is thrown overboard by her father during a storm. When she tries to climb back in, he cuts off her fingers. As she sinks, her fingers become the seals, walruses, and whales.", "The Legend of Sedna" },
                    { 34, 52, "Sedna becomes the Mother of the Sea, ruling all marine life from her home at the bottom of the ocean. Hunters must now respect her to ensure a successful hunt.", "The Legend of Sedna" },
                    { 35, 53, "Cinderella is forced into servitude by her stepmother and stepsisters, living among the ashes and performing backbreaking chores.", "Cinderella" },
                    { 36, 54, "With the help of her fairy godmother, she attends the Royal Ball in a magical gown and glass slippers, dancing with the Prince.", "Cinderella" },
                    { 37, 55, "At midnight, the magic fades. She flees, leaving only a single glass slipper behind, and returns to her life of rags.", "Cinderella" },
                    { 38, 57, "Jason is commanded by King Pelias to retrieve the legendary Golden Fleece from the distant land of Colchis to reclaim his throne.", "Jason and the Golden Fleece" },
                    { 39, 60, "After surviving the Symplegades and completing the impossible tasks set by King Aeetes, Jason retrieves the Fleece from the sleeping dragon.", "Jason and the Golden Fleece" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountApiKeyHistories_AccountId",
                table: "AccountApiKeyHistories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ActiveThemeId",
                table: "Accounts",
                column: "ActiveThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypeExamples_ArchetypePointId",
                table: "ArchetypeExamples",
                column: "ArchetypePointId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypePoints_ArchetypeId",
                table: "ArchetypePoints",
                column: "ArchetypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntities_NotebookId",
                table: "NotebookEntities",
                column: "NotebookId");

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntries_NotebookEntityId",
                table: "NotebookEntries",
                column: "NotebookEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_NotebookEntries_StoryId",
                table: "NotebookEntries",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Notebooks_AccountId",
                table: "Notebooks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_AccountId",
                table: "Stories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryEntityLinks_NotebookEntityId",
                table: "StoryEntityLinks",
                column: "NotebookEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountApiKeyHistories");

            migrationBuilder.DropTable(
                name: "ArchetypeExamples");

            migrationBuilder.DropTable(
                name: "NotebookEntries");

            migrationBuilder.DropTable(
                name: "NotebookTypes");

            migrationBuilder.DropTable(
                name: "StoryEntityLinks");

            migrationBuilder.DropTable(
                name: "ArchetypePoints");

            migrationBuilder.DropTable(
                name: "NotebookEntities");

            migrationBuilder.DropTable(
                name: "Stories");

            migrationBuilder.DropTable(
                name: "Archetypes");

            migrationBuilder.DropTable(
                name: "Notebooks");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
