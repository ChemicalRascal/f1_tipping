using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddMultiEntityResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Results",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MultiEntityResultRacingEntity",
                columns: table => new
                {
                    ResultHoldersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MultiEntityResultEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MultiEntityResultType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiEntityResultRacingEntity", x => new { x.ResultHoldersId, x.MultiEntityResultEventId, x.MultiEntityResultType });
                    table.ForeignKey(
                        name: "FK_MultiEntityResultRacingEntity_RacingEntities_ResultHoldersId",
                        column: x => x.ResultHoldersId,
                        principalTable: "RacingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiEntityResultRacingEntity_Results_MultiEntityResultEventId_MultiEntityResultType",
                        columns: x => new { x.MultiEntityResultEventId, x.MultiEntityResultType },
                        principalTable: "Results",
                        principalColumns: new[] { "EventId", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultiEntityResultRacingEntity_MultiEntityResultEventId_MultiEntityResultType",
                table: "MultiEntityResultRacingEntity",
                columns: new[] { "MultiEntityResultEventId", "MultiEntityResultType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MultiEntityResultRacingEntity");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Results");
        }
    }
}
