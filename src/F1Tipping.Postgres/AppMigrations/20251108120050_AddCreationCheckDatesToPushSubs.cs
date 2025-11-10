using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.AppMigrations
{
    /// <inheritdoc />
    public partial class AddCreationCheckDatesToPushSubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "userpushnotificationsubscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "lastcheck",
                table: "userpushnotificationsubscriptions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created",
                table: "userpushnotificationsubscriptions");

            migrationBuilder.DropColumn(
                name: "lastcheck",
                table: "userpushnotificationsubscriptions");
        }
    }
}
