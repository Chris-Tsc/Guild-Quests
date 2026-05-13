using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQuestOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestOptions");

            migrationBuilder.CreateTable(
                name: "DailyQuestOptions",
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
                    DailyQuestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuestOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuestOptions_DailyQuests_DailyQuestId",
                        column: x => x.DailyQuestId,
                        principalTable: "DailyQuests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuestOptions_DailyQuestId",
                table: "DailyQuestOptions",
                column: "DailyQuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuestOptions");

            migrationBuilder.CreateTable(
                name: "QuestOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgilityWeight = table.Column<int>(type: "int", nullable: false),
                    BaseSuccessChance = table.Column<int>(type: "int", nullable: false),
                    IntelligenceWeight = table.Column<int>(type: "int", nullable: false),
                    PerceptionWeight = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    QuestType = table.Column<int>(type: "int", nullable: true),
                    StrengthWeight = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestOptions", x => x.Id);
                });
        }
    }
}
