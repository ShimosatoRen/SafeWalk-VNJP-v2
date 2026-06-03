using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetStarterProject.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToDangerSpot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DangerSpots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DangerSpots");
        }
    }
}
