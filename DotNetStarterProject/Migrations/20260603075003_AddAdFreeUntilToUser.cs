using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetStarterProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAdFreeUntilToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DangerLevel",
                table: "DangerSpots");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdFreeUntil",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdFreeUntil",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "DangerLevel",
                table: "DangerSpots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
