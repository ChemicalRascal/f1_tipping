using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.AppMigrations
{
    /// <inheritdoc />
    public partial class AddUserExtraSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserExtraSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationsSettings_TipDeadlineStartOffset = table.Column<TimeSpan>(type: "time", nullable: true),
                    NotificationsSettings_NotifyForOldTips = table.Column<bool>(type: "bit", nullable: true),
                    NotificationsSettings_ScheduleType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExtraSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserExtraSettings_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExtraSettings");
        }
    }
}
