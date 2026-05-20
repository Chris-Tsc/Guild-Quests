using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerRolledGuildQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerRolledGuildQuests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    GuildQuestId = table.Column<int>(type: "int", nullable: false),
                    DaytimeInfoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRolledGuildQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerRolledGuildQuests_GuildQuests_GuildQuestId",
                        column: x => x.GuildQuestId,
                        principalTable: "GuildQuests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerRolledGuildQuests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRolledGuildQuests_GuildQuestId",
                table: "PlayerRolledGuildQuests",
                column: "GuildQuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRolledGuildQuests_PlayerId_DaytimeInfoUtc_GuildQuestId",
                table: "PlayerRolledGuildQuests",
                columns: new[] { "PlayerId", "DaytimeInfoUtc", "GuildQuestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerRolledGuildQuests");
        }
    }
}
