using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StravaRaceAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "Tokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Tokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClientSecret",
                table: "Tokens",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
