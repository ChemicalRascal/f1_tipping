using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    /// <inheritdoc />
    public partial class FixTipDeadlinesAndSplitDriversAndDriverTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DriverId",
                table: "RacingEntities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RacingEntities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TipsDeadline",
                table: "Events",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_RacingEntities_DriverId",
                table: "RacingEntities",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_RacingEntities_Drivers_DriverId",
                table: "RacingEntities",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.Sql(@"
                    insert into drivers
                        (id, firstname, lastname, nationality, number)
                    (select gen_random_uuid(), firstname, lastname,
                            nationality, number from racingentities
                            where discriminator = 'Driver');");

            migrationBuilder.Sql(@"
                    update racingentities re
                        set discriminator = 'DriverTeam'
                        , driverid = d.id
                        , status = 1
                    from drivers d
                        where re.lastname = d.lastname
                        and re.discriminator = 'Driver';");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "RacingEntities");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "RacingEntities");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "RacingEntities");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "RacingEntities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "RacingEntities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "RacingEntities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "RacingEntities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "RacingEntities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
                    update racingentities re
                        set discriminator = 'Driver'
                        , firstname = d.firstname
                        , lastname = d.lastname
                        , nationality = d.nationality
                        , number = d.number
                        , driverid = d.id
                    from drivers d
                    where re.driverid = d.id
                    and re.discriminator = 'DriverTeam';");

            migrationBuilder.DropForeignKey(
                name: "FK_RacingEntities_Drivers_DriverId",
                table: "RacingEntities");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_RacingEntities_DriverId",
                table: "RacingEntities");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "RacingEntities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RacingEntities");

            migrationBuilder.DropColumn(
                name: "TipsDeadline",
                table: "Events");
        }
    }
}
