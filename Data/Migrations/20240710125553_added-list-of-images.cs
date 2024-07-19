using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoliPics.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedlistofimages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Albums_AlbumId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_AlbumId",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Images",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Images_AlbumId",
                table: "Images",
                column: "AlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Albums_AlbumId",
                table: "Images",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id");
        }
    }
}
