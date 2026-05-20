using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerGuildQuestAcceptedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerGuildQuests_PlayerId",
                table: "PlayerGuildQuests");

            migrationBuilder.AddColumn<DateTime>(
                name: "DaytimeInfoUtc",
                table: "PlayerGuildQuests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGuildQuests_PlayerId_DaytimeInfoUtc_GuildQuestId",
                table: "PlayerGuildQuests",
                columns: new[] { "PlayerId", "DaytimeInfoUtc", "GuildQuestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerGuildQuests_PlayerId_DaytimeInfoUtc_GuildQuestId",
                table: "PlayerGuildQuests");

            migrationBuilder.DropColumn(
                name: "DaytimeInfoUtc",
                table: "PlayerGuildQuests");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGuildQuests_PlayerId",
                table: "PlayerGuildQuests",
                column: "PlayerId");
        }
    }
}
