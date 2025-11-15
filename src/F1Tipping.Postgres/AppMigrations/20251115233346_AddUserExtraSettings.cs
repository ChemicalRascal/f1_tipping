using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.AppMigrations
{
    /// <inheritdoc />
    public partial class AddUserExtraSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userextrasettings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notificationssettings_tipdeadlinestartoffset = table.Column<TimeSpan>(type: "interval", nullable: true),
                    notificationssettings_notifyforoldtips = table.Column<bool>(type: "boolean", nullable: true),
                    notificationssettings_scheduletype = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_userextrasettings", x => x.id);
                    table.ForeignKey(
                        name: "fk_userextrasettings_users_id",
                        column: x => x.id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userextrasettings");
        }
    }
}
