using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.AppMigrations
{
    /// <inheritdoc />
    public partial class TrackPushSubErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "UserPushNotificationSubscriptions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "UserPushNotificationSubscriptions");
        }
    }
}
