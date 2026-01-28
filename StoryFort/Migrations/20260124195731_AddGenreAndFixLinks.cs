using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreAndFixLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OllamaUrl",
                table: "Accounts",
                newName: "SupervisorName");

            migrationBuilder.RenameColumn(
                name: "OllamaModel",
                table: "Accounts",
                newName: "SupervisorEmail");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Stories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Stories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "NotebookEntities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CohereApiKey",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseReasoningModel",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CohereApiKey", "SupervisorEmail", "SupervisorName", "UseReasoningModel" },
                values: new object[] { "", "", "", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "NotebookEntities");

            migrationBuilder.DropColumn(
                name: "CohereApiKey",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "UseReasoningModel",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "SupervisorName",
                table: "Accounts",
                newName: "OllamaUrl");

            migrationBuilder.RenameColumn(
                name: "SupervisorEmail",
                table: "Accounts",
                newName: "OllamaModel");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "OllamaModel", "OllamaUrl" },
                values: new object[] { "llama3", "http://localhost:11434" });
        }
    }
}

