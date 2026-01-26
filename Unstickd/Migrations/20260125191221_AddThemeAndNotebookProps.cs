using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstickd.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeAndNotebookProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Notebooks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Notebooks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ThemePreferenceJson",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "ThemePreferenceJson",
                value: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Notebooks");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Notebooks");

            migrationBuilder.DropColumn(
                name: "ThemePreferenceJson",
                table: "Accounts");
        }
    }
}
