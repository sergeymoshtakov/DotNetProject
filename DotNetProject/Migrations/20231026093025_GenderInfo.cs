using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetProject.Migrations
{
    /// <inheritdoc />
    public partial class GenderInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_IdGender",
                table: "Users",
                column: "IdGender");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Genders_IdGender",
                table: "Users",
                column: "IdGender",
                principalTable: "Genders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Genders_IdGender",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IdGender",
                table: "Users");
        }
    }
}
