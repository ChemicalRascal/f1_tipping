using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.ModelMigrations
{
    /// <inheritdoc />
    public partial class DifferentiateDriversAndDriverTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "driverid",
                table: "racingentities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "racingentities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    nationality = table.Column<string>(type: "text", nullable: false),
                    number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drivers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_racingentities_driverid",
                table: "racingentities",
                column: "driverid");

            migrationBuilder.AddForeignKey(
                name: "fk_racingentities_drivers_driverid",
                table: "racingentities",
                column: "driverid",
                principalTable: "drivers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "firstname",
                table: "racingentities");

            migrationBuilder.DropColumn(
                name: "lastname",
                table: "racingentities");

            migrationBuilder.DropColumn(
                name: "nationality",
                table: "racingentities");

            migrationBuilder.DropColumn(
                name: "number",
                table: "racingentities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "firstname",
                table: "racingentities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lastname",
                table: "racingentities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                table: "racingentities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "racingentities",
                type: "text",
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
                name: "fk_racingentities_drivers_driverid",
                table: "racingentities");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.DropIndex(
                name: "ix_racingentities_driverid",
                table: "racingentities");

            migrationBuilder.DropColumn(
                name: "driverid",
                table: "racingentities");

            migrationBuilder.DropColumn(
                name: "status",
                table: "racingentities");
        }
    }
}
