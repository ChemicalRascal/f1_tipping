using System;
using System.Collections.Generic;
using F1Tipping.Model.Tipping;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.ModelMigrations
{
    /// <inheritdoc />
    public partial class InitModelDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    authuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    details_firstname = table.Column<string>(type: "text", nullable: true),
                    details_lastname = table.Column<string>(type: "text", nullable: true),
                    details_displayname = table.Column<string>(type: "text", nullable: true),
                    additionalauthedusers = table.Column<List<Guid>>(type: "uuid[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_players", x => x.id);
                    table.CheckConstraint(
                            name: "ck_players_status_implies_firstname_exists",
                            sql: $"status = {(int)PlayerStatus.Uninitialized} OR details_firstname IS NOT NULL"
                        );
                });

            migrationBuilder.CreateTable(
                name: "racingentities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    listorder = table.Column<int>(type: "integer", nullable: true),
                    discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: true),
                    lastname = table.Column<string>(type: "text", nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    number = table.Column<string>(type: "text", nullable: true),
                    teamid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_racingentities", x => x.id);
                    table.ForeignKey(
                        name: "fk_racingentities_racingentities_teamid",
                        column: x => x.teamid,
                        principalTable: "racingentities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed = table.Column<bool>(type: "boolean", nullable: false),
                    discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: true),
                    weekendid = table.Column<Guid>(type: "uuid", nullable: true),
                    racestart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    qualificationstart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    type = table.Column<int>(type: "integer", nullable: false),
                    eventid = table.Column<Guid>(type: "uuid", nullable: false),
                    resultholderid = table.Column<Guid>(type: "uuid", nullable: true),
                    set = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    setbyauthuser = table.Column<Guid>(type: "uuid", nullable: true),
                    discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_results", x => new { x.eventid, x.type });
                    table.ForeignKey(
                        name: "fk_results_events_eventid",
                        column: x => x.eventid,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_results_racingentities_resultholderid",
                        column: x => x.resultholderid,
                        principalTable: "racingentities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    seasonid = table.Column<Guid>(type: "uuid", nullable: false),
                    index = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    startdate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rounds", x => x.id);
                    table.ForeignKey(
                        name: "fk_rounds_seasons_seasonid",
                        column: x => x.seasonid,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "multientityresultracingentity",
                columns: table => new
                {
                    resultholdersid = table.Column<Guid>(type: "uuid", nullable: false),
                    multientityresulteventid = table.Column<Guid>(type: "uuid", nullable: false),
                    multientityresulttype = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_multientityresultracingentity", x => new { x.resultholdersid, x.multientityresulteventid, x.multientityresulttype });
                    table.ForeignKey(
                        name: "fk_multientityresultracingentity_racingentities_resultholdersid",
                        column: x => x.resultholdersid,
                        principalTable: "racingentities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_multientityresultracingentity_results_multientityresulteven~",
                        columns: x => new { x.multientityresulteventid, x.multientityresulttype },
                        principalTable: "results",
                        principalColumns: new[] { "eventid", "type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tips",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipperid = table.Column<Guid>(type: "uuid", nullable: false),
                    targeteventid = table.Column<Guid>(type: "uuid", nullable: false),
                    targettype = table.Column<int>(type: "integer", nullable: false),
                    selectionid = table.Column<Guid>(type: "uuid", nullable: false),
                    submittedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submittedby_authuser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tips", x => x.id);
                    table.ForeignKey(
                        name: "fk_tips_players_tipperid",
                        column: x => x.tipperid,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tips_racingentities_selectionid",
                        column: x => x.selectionid,
                        principalTable: "racingentities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tips_results_targeteventid_targettype",
                        columns: x => new { x.targeteventid, x.targettype },
                        principalTable: "results",
                        principalColumns: new[] { "eventid", "type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_weekendid",
                table: "events",
                column: "weekendid");

            migrationBuilder.CreateIndex(
                name: "ix_multientityresultracingentity_multientityresulteventid_mult~",
                table: "multientityresultracingentity",
                columns: new[] { "multientityresulteventid", "multientityresulttype" });

            migrationBuilder.CreateIndex(
                name: "ix_racingentities_teamid",
                table: "racingentities",
                column: "teamid");

            migrationBuilder.CreateIndex(
                name: "ix_results_resultholderid",
                table: "results",
                column: "resultholderid");

            migrationBuilder.CreateIndex(
                name: "ix_rounds_seasonid",
                table: "rounds",
                column: "seasonid");

            migrationBuilder.CreateIndex(
                name: "ix_tips_selectionid",
                table: "tips",
                column: "selectionid");

            migrationBuilder.CreateIndex(
                name: "ix_tips_targeteventid_targettype",
                table: "tips",
                columns: new[] { "targeteventid", "targettype" });

            migrationBuilder.CreateIndex(
                name: "ix_tips_tipperid",
                table: "tips",
                column: "tipperid");

            migrationBuilder.AddForeignKey(
                name: "fk_events_rounds_weekendid",
                table: "events",
                column: "weekendid",
                principalTable: "rounds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_events_rounds_weekendid",
                table: "events");

            migrationBuilder.DropTable(
                name: "multientityresultracingentity");

            migrationBuilder.DropTable(
                name: "tips");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "racingentities");

            migrationBuilder.DropTable(
                name: "rounds");

            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
