using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.AppMigrations
{
    /// <inheritdoc />
    public partial class AddOwnUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "settings_notificationssettings_notifyforoldtips",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "settings_notificationssettings_scheduletype",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "settings_notificationssettings_tipdeadlinestartoffset",
                table: "AspNetUsers",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings_notificationssettings_notifyforoldtips",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "settings_notificationssettings_scheduletype",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "settings_notificationssettings_tipdeadlinestartoffset",
                table: "AspNetUsers");
        }
    }
}
