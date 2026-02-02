using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryFort.Migrations
{
    /// <inheritdoc />
    public partial class AutoCapturePendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CohereApiKey",
                table: "Accounts",
                newName: "ProtectedCohereApiKey");

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

            migrationBuilder.CreateIndex(
                name: "IX_AccountApiKeyHistories_AccountId",
                table: "AccountApiKeyHistories",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountApiKeyHistories");

            migrationBuilder.RenameColumn(
                name: "ProtectedCohereApiKey",
                table: "Accounts",
                newName: "CohereApiKey");
        }
    }
}
