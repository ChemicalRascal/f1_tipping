using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Tipping.Postgres.ModelMigrations
{
    /// <inheritdoc />
    public partial class MakeTipDeadlinesFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "tipsdeadline",
                table: "events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql(@"update events eActual
                set tipsdeadline = deadlines.deadline
                from (select weekendid, min(qualificationstart) as deadline from events group by weekendid) deadlines
                where eActual.weekendid = deadlines.weekendid;");

            // Fuck it we're hardcoding this one
            migrationBuilder.Sql(@"update events eActual
                set tipsdeadline = '2025-03-15 16:00:00+11'
                where eActual.discriminator = 'Season' and eActual.year = 2025;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipsdeadline",
                table: "events");
        }
    }
}
