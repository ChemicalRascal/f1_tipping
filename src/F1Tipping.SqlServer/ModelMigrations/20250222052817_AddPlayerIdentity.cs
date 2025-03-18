using System;
using F1Tipping.Model.Tipping;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddPlayerIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Players_Status_Implies_Identity_Exists",
                table: "Players"
                );

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Identity_DetailsId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "Identity");

            migrationBuilder.DropIndex(
                name: "IX_Players_DetailsId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DetailsId",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "Details_DisplayName",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details_FirstName",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details_LastName",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Players_Status_Implies_FirstName_NotNull",
                table: "Players",
                sql: $"Status = {(int)PlayerStatus.Uninitialized} OR Details_FirstName IS NOT NULL"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details_DisplayName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Details_FirstName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Details_LastName",
                table: "Players");

            migrationBuilder.AddColumn<Guid>(
                name: "DetailsId",
                table: "Players",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_DetailsId",
                table: "Players",
                column: "DetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Identity_DetailsId",
                table: "Players",
                column: "DetailsId",
                principalTable: "Identity",
                principalColumn: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Players_Status_Implies_Identity_Exists",
                table: "Players",
                sql: $"Status = {(int)PlayerStatus.Uninitialized} OR DetailsId IS NOT NULL"
                );
        }
    }
}
