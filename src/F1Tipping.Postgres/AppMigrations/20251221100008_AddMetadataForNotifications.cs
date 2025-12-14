using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace F1Tipping.Postgres.AppMigrations
{
    /// <inheritdoc />
    public partial class AddMetadataForNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "settings_notificationssettings_minimumtimebetweennotifications",
                table: "AspNetUsers",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "temporaldata_lastnotification",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "temporaldata_nextnotification",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "globaltemporaldata",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lastnotifiedroundid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_globaltemporaldata", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "globaltemporaldata");

            migrationBuilder.DropColumn(
                name: "settings_notificationssettings_minimumtimebetweennotifications",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "temporaldata_lastnotification",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "temporaldata_nextnotification",
                table: "AspNetUsers");
        }
    }
}
