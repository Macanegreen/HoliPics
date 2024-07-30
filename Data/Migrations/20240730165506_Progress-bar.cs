using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoliPics.Data.Migrations
{
    /// <inheritdoc />
    public partial class Progressbar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploadProgress",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadProgress",
                table: "Albums");
        }
    }
}
