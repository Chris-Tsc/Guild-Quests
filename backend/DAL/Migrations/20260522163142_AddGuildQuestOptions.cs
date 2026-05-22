using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildQuestOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildQuestOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrengthWeight = table.Column<int>(type: "int", nullable: false),
                    IntelligenceWeight = table.Column<int>(type: "int", nullable: false),
                    AgilityWeight = table.Column<int>(type: "int", nullable: false),
                    PerceptionWeight = table.Column<int>(type: "int", nullable: false),
                    BaseSuccessChance = table.Column<int>(type: "int", nullable: false),
                    GuildQuestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildQuestOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildQuestOptions_GuildQuests_GuildQuestId",
                        column: x => x.GuildQuestId,
                        principalTable: "GuildQuests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildQuestOptions_GuildQuestId",
                table: "GuildQuestOptions",
                column: "GuildQuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildQuestOptions");
        }
    }
}
