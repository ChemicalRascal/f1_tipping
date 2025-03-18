using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddProperTipSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Debug_Tip",
                table: "Tips");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectionId",
                table: "Tips",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tips_SelectionId",
                table: "Tips",
                column: "SelectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tips_RacingEntities_SelectionId",
                table: "Tips",
                column: "SelectionId",
                principalTable: "RacingEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tips_RacingEntities_SelectionId",
                table: "Tips");

            migrationBuilder.DropIndex(
                name: "IX_Tips_SelectionId",
                table: "Tips");

            migrationBuilder.DropColumn(
                name: "SelectionId",
                table: "Tips");

            migrationBuilder.AddColumn<string>(
                name: "Debug_Tip",
                table: "Tips",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
