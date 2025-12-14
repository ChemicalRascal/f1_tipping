using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.AppMigrations
{
    /// <inheritdoc />
    public partial class AddMetadataForNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "NotificationsSettings_MinimumTimeBetweenNotifications",
                table: "UserExtraSettings",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TemporalData_LastNotification",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TemporalData_NextNotification",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlobalTemporalData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastNotifiedRoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalTemporalData", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalTemporalData");

            migrationBuilder.DropColumn(
                name: "NotificationsSettings_MinimumTimeBetweenNotifications",
                table: "UserExtraSettings");

            migrationBuilder.DropColumn(
                name: "TemporalData_LastNotification",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TemporalData_NextNotification",
                table: "AspNetUsers");
        }
    }
}
