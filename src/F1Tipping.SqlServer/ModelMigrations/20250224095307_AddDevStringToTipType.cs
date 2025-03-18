using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class AddDevStringToTipType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Debug_Tip",
                table: "Tips",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Debug_Tip",
                table: "Tips");
        }
    }
}
