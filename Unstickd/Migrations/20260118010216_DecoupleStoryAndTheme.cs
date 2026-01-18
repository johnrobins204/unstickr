using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstickd.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleStoryAndTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stories_Themes_ThemeId",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_ThemeId",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "Stories");

            migrationBuilder.AddColumn<int>(
                name: "ActiveThemeId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActiveThemeId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ActiveThemeId",
                table: "Accounts",
                column: "ActiveThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Themes_ActiveThemeId",
                table: "Accounts",
                column: "ActiveThemeId",
                principalTable: "Themes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Themes_ActiveThemeId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ActiveThemeId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ActiveThemeId",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "Stories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Stories_ThemeId",
                table: "Stories",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_Themes_ThemeId",
                table: "Stories",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
