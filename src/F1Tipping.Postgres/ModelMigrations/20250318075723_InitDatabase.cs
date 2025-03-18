using F1Tipping.Model.Tipping;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.ModelMigrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Details_FirstName = table.Column<string>(type: "text", nullable: true),
                    Details_LastName = table.Column<string>(type: "text", nullable: true),
                    Details_DisplayName = table.Column<string>(type: "text", nullable: true),
                    AdditionalAuthedUsers = table.Column<List<Guid>>(type: "uuid[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.CheckConstraint(
                        name: "CK_Players_Status_Implies_Identity_Initialized",
                        sql: $"\"Status\" = {(int)PlayerStatus.Uninitialized} OR \"Details_FirstName\" IS NOT NULL"
                        );
                });

            migrationBuilder.CreateTable(
                name: "RacingEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListOrder = table.Column<int>(type: "integer", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: true),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RacingEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RacingEntities_RacingEntities_TeamId",
                        column: x => x.TeamId,
                        principalTable: "RacingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    WeekendId = table.Column<Guid>(type: "uuid", nullable: true),
                    RaceStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    QualificationStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultHolderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Set = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SetByAuthUser = table.Column<Guid>(type: "uuid", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => new { x.EventId, x.Type });
                    table.ForeignKey(
                        name: "FK_Results_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_RacingEntities_ResultHolderId",
                        column: x => x.ResultHolderId,
                        principalTable: "RacingEntities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Events_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiEntityResultRacingEntity",
                columns: table => new
                {
                    ResultHoldersId = table.Column<Guid>(type: "uuid", nullable: false),
                    MultiEntityResultEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    MultiEntityResultType = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_MultiEntityResultRacingEntity_Results_MultiEntityResultEven~",
                        columns: x => new { x.MultiEntityResultEventId, x.MultiEntityResultType },
                        principalTable: "Results",
                        principalColumns: new[] { "EventId", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TipperId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    SelectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmittedBy_AuthUser = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tips_Players_TipperId",
                        column: x => x.TipperId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tips_RacingEntities_SelectionId",
                        column: x => x.SelectionId,
                        principalTable: "RacingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tips_Results_TargetEventId_TargetType",
                        columns: x => new { x.TargetEventId, x.TargetType },
                        principalTable: "Results",
                        principalColumns: new[] { "EventId", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_WeekendId",
                table: "Events",
                column: "WeekendId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiEntityResultRacingEntity_MultiEntityResultEventId_Mult~",
                table: "MultiEntityResultRacingEntity",
                columns: new[] { "MultiEntityResultEventId", "MultiEntityResultType" });

            migrationBuilder.CreateIndex(
                name: "IX_RacingEntities_TeamId",
                table: "RacingEntities",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ResultHolderId",
                table: "Results",
                column: "ResultHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_SeasonId",
                table: "Rounds",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Tips_SelectionId",
                table: "Tips",
                column: "SelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tips_TargetEventId_TargetType",
                table: "Tips",
                columns: new[] { "TargetEventId", "TargetType" });

            migrationBuilder.CreateIndex(
                name: "IX_Tips_TipperId",
                table: "Tips",
                column: "TipperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Rounds_WeekendId",
                table: "Events",
                column: "WeekendId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Rounds_WeekendId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "MultiEntityResultRacingEntity");

            migrationBuilder.DropTable(
                name: "Tips");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "RacingEntities");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
