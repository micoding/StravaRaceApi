using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StravaRaceAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceSegment_Events_RaceId",
                table: "RaceSegment");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "RaceId",
                table: "RaceSegment",
                newName: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceSegment_Events_EventId",
                table: "RaceSegment",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceSegment_Events_EventId",
                table: "RaceSegment");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "RaceSegment",
                newName: "RaceId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Events",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceSegment_Events_RaceId",
                table: "RaceSegment",
                column: "RaceId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
