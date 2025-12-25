using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddStatusesToDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "racingentities",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "drivers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update drivers set status = 1 where status = 0");
            migrationBuilder.Sql("update racingentities set status = 1 where status = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "drivers");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "racingentities",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
