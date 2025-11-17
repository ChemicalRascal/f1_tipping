using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.AppMigrations
{
    /// <inheritdoc />
    public partial class AddOwnUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExtraSettings_AspNetUsers_Id",
                table: "UserExtraSettings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserExtraSettings",
                newName: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserExtraSettings_AspNetUsers_UserId",
                table: "UserExtraSettings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExtraSettings_AspNetUsers_UserId",
                table: "UserExtraSettings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserExtraSettings",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserExtraSettings_AspNetUsers_Id",
                table: "UserExtraSettings",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
