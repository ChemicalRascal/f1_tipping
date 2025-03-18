using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddSeasonRoundRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Rounds");

            migrationBuilder.AddColumn<Guid>(
                name: "SeasonId",
                table: "Rounds",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDate",
                table: "Rounds",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Events");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_SeasonId",
                table: "Rounds",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Events_SeasonId",
                table: "Rounds",
                column: "SeasonId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Events_SeasonId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_SeasonId",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Rounds");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Rounds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Year",
                table: "Events",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
