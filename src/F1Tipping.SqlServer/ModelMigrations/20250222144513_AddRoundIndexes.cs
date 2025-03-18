using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddRoundIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Rounds",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Rounds");
        }
    }
}
