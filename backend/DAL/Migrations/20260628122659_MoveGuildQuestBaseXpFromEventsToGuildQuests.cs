using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class MoveGuildQuestBaseXpFromEventsToGuildQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseXP",
                table: "GuildQuests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE g
                SET g.BaseXP = e.BaseXP
                FROM GuildQuests g
                INNER JOIN Events e ON g.EventsId = e.Id");


            migrationBuilder.DropColumn(
                name: "BaseXP",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseXP",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE e
                SET e.BaseXP = g.BaseXP
                FROM Events e
                INNER JOIN GuildQuests g ON g.EventsId = e.Id");

            migrationBuilder.DropColumn(
                name: "BaseXP",
                table: "GuildQuests");
        }
    }
}
