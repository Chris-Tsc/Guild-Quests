using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQuestsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseXP",
                table: "DailyQuests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlayerDailyQuests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    DailyQuestId = table.Column<int>(type: "int", nullable: false),
                    DaytimeInfoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerDailyQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerDailyQuests_DailyQuests_DailyQuestId",
                        column: x => x.DailyQuestId,
                        principalTable: "DailyQuests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerDailyQuests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDailyQuests_DailyQuestId",
                table: "PlayerDailyQuests",
                column: "DailyQuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDailyQuests_PlayerId",
                table: "PlayerDailyQuests",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerDailyQuests");

            migrationBuilder.DropColumn(
                name: "BaseXP",
                table: "DailyQuests");
        }
    }
}
