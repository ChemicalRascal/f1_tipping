using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddStatusesToDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "RacingEntities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update drivers set status = 1 where status = 0");
            migrationBuilder.Sql("update racingentities set status = 1 where status = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Drivers");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "RacingEntities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
