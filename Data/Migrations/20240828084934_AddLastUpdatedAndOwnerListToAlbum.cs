using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoliPics.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastUpdatedAndOwnerListToAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Albums",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Owners",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Owners",
                table: "Albums");
        }
    }
}
