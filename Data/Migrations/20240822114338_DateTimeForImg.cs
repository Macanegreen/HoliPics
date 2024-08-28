using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoliPics.Data.Migrations
{
    /// <inheritdoc />
    public partial class DateTimeForImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTaken",
                table: "Images",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTaken",
                table: "Images");
        }
    }
}
