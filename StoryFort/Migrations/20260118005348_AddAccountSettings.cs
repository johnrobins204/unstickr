using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OllamaModel",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OllamaUrl",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "OllamaModel", "OllamaUrl" },
                values: new object[] { "llama3", "http://localhost:11434" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OllamaModel",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "OllamaUrl",
                table: "Accounts");
        }
    }
}

