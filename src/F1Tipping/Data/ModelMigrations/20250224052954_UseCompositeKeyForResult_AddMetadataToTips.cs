using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class UseCompositeKeyForResult_AddMetadataToTips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Events_EventId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Tips_Results_TargetId",
                table: "Tips");

            migrationBuilder.DropIndex(
                name: "IX_Tips_TargetId",
                table: "Tips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Results",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_EventId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "Tips",
                newName: "TargetEventId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedAt",
                table: "Tips",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedBy_AuthUser",
                table: "Tips",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                table: "Tips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "Results",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Results",
                table: "Results",
                columns: new[] { "EventId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Tips_TargetEventId_TargetType",
                table: "Tips",
                columns: new[] { "TargetEventId", "TargetType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Events_EventId",
                table: "Results",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tips_Results_TargetEventId_TargetType",
                table: "Tips",
                columns: new[] { "TargetEventId", "TargetType" },
                principalTable: "Results",
                principalColumns: new[] { "EventId", "Type" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Events_EventId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Tips_Results_TargetEventId_TargetType",
                table: "Tips");

            migrationBuilder.DropIndex(
                name: "IX_Tips_TargetEventId_TargetType",
                table: "Tips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Results",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Tips");

            migrationBuilder.DropColumn(
                name: "SubmittedBy_AuthUser",
                table: "Tips");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "Tips");

            migrationBuilder.RenameColumn(
                name: "TargetEventId",
                table: "Tips",
                newName: "TargetId");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "Results",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Results",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Results",
                table: "Results",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tips_TargetId",
                table: "Tips",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_EventId",
                table: "Results",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Events_EventId",
                table: "Results",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tips_Results_TargetId",
                table: "Tips",
                column: "TargetId",
                principalTable: "Results",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
