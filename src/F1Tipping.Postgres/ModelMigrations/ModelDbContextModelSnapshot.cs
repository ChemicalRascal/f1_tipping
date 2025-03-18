﻿// <auto-generated />
using System;
using System.Collections.Generic;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace F1Tipping.Postgres.ModelMigrations
{
    [DbContext(typeof(ModelDbContext))]
    partial class ModelDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("F1Tipping.Model.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Completed")
                        .HasColumnType("boolean")
                        .HasColumnName("completed");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("character varying(8)")
                        .HasColumnName("discriminator");

                    b.Property<DateTimeOffset>("TipsDeadline")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("tipsdeadline");

                    b.HasKey("Id")
                        .HasName("pk_events");

                    b.ToTable("events", (string)null);

                    b.HasDiscriminator().HasValue("Event");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("F1Tipping.Model.RacingEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("character varying(13)")
                        .HasColumnName("discriminator");

                    b.Property<int?>("ListOrder")
                        .HasColumnType("integer")
                        .HasColumnName("listorder");

                    b.HasKey("Id")
                        .HasName("pk_racingentities");

                    b.ToTable("racingentities", (string)null);

                    b.HasDiscriminator().HasValue("RacingEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("F1Tipping.Model.Result", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid")
                        .HasColumnName("eventid");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("character varying(21)")
                        .HasColumnName("discriminator");

                    b.Property<Guid?>("ResultHolderId")
                        .HasColumnType("uuid")
                        .HasColumnName("resultholderid");

                    b.Property<DateTimeOffset?>("Set")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("set");

                    b.Property<Guid?>("SetByAuthUser")
                        .HasColumnType("uuid")
                        .HasColumnName("setbyauthuser");

                    b.HasKey("EventId", "Type")
                        .HasName("pk_results");

                    b.HasIndex("ResultHolderId")
                        .HasDatabaseName("ix_results_resultholderid");

                    b.ToTable("results", (string)null);

                    b.HasDiscriminator().HasValue("Result");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("F1Tipping.Model.Round", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("Index")
                        .HasColumnType("integer")
                        .HasColumnName("index");

                    b.Property<Guid>("SeasonId")
                        .HasColumnType("uuid")
                        .HasColumnName("seasonid");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("startdate");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_rounds");

                    b.HasIndex("SeasonId")
                        .HasDatabaseName("ix_rounds_seasonid");

                    b.ToTable("rounds", (string)null);
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.PrimitiveCollection<List<Guid>>("AdditionalAuthedUsers")
                        .HasColumnType("uuid[]")
                        .HasColumnName("additionalauthedusers");

                    b.Property<Guid>("AuthUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("authuserid");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_players");

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Tip", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("SelectionId")
                        .HasColumnType("uuid")
                        .HasColumnName("selectionid");

                    b.Property<DateTimeOffset>("SubmittedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("submittedat");

                    b.Property<Guid?>("SubmittedBy_AuthUser")
                        .HasColumnType("uuid")
                        .HasColumnName("submittedby_authuser");

                    b.Property<Guid>("TargetEventId")
                        .HasColumnType("uuid")
                        .HasColumnName("targeteventid");

                    b.Property<int>("TargetType")
                        .HasColumnType("integer")
                        .HasColumnName("targettype");

                    b.Property<Guid>("TipperId")
                        .HasColumnType("uuid")
                        .HasColumnName("tipperid");

                    b.HasKey("Id")
                        .HasName("pk_tips");

                    b.HasIndex("SelectionId")
                        .HasDatabaseName("ix_tips_selectionid");

                    b.HasIndex("TipperId")
                        .HasDatabaseName("ix_tips_tipperid");

                    b.HasIndex("TargetEventId", "TargetType")
                        .HasDatabaseName("ix_tips_targeteventid_targettype");

                    b.ToTable("tips", (string)null);
                });

            modelBuilder.Entity("MultiEntityResultRacingEntity", b =>
                {
                    b.Property<Guid>("ResultHoldersId")
                        .HasColumnType("uuid")
                        .HasColumnName("resultholdersid");

                    b.Property<Guid>("MultiEntityResultEventId")
                        .HasColumnType("uuid")
                        .HasColumnName("multientityresulteventid");

                    b.Property<int>("MultiEntityResultType")
                        .HasColumnType("integer")
                        .HasColumnName("multientityresulttype");

                    b.HasKey("ResultHoldersId", "MultiEntityResultEventId", "MultiEntityResultType")
                        .HasName("pk_multientityresultracingentity");

                    b.HasIndex("MultiEntityResultEventId", "MultiEntityResultType")
                        .HasDatabaseName("ix_multientityresultracingentity_multientityresulteventid_mult~");

                    b.ToTable("multientityresultracingentity", (string)null);
                });

            modelBuilder.Entity("F1Tipping.Model.Race", b =>
                {
                    b.HasBaseType("F1Tipping.Model.Event");

                    b.Property<DateTimeOffset>("QualificationStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("qualificationstart");

                    b.Property<DateTimeOffset>("RaceStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("racestart");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<Guid>("WeekendId")
                        .HasColumnType("uuid")
                        .HasColumnName("weekendid");

                    b.HasIndex("WeekendId")
                        .HasDatabaseName("ix_events_weekendid");

                    b.HasDiscriminator().HasValue("Race");
                });

            modelBuilder.Entity("F1Tipping.Model.Season", b =>
                {
                    b.HasBaseType("F1Tipping.Model.Event");

                    b.Property<int>("Year")
                        .HasColumnType("integer")
                        .HasColumnName("year");

                    b.HasDiscriminator().HasValue("Season");
                });

            modelBuilder.Entity("F1Tipping.Model.Driver", b =>
                {
                    b.HasBaseType("F1Tipping.Model.RacingEntity");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("firstname");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("lastname");

                    b.Property<string>("Nationality")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("nationality");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("number");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid")
                        .HasColumnName("teamid");

                    b.HasIndex("TeamId")
                        .HasDatabaseName("ix_racingentities_teamid");

                    b.HasDiscriminator().HasValue("Driver");
                });

            modelBuilder.Entity("F1Tipping.Model.Team", b =>
                {
                    b.HasBaseType("F1Tipping.Model.RacingEntity");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasDiscriminator().HasValue("Team");
                });

            modelBuilder.Entity("F1Tipping.Model.MultiEntityResult", b =>
                {
                    b.HasBaseType("F1Tipping.Model.Result");

                    b.ToTable("results", (string)null);

                    b.HasDiscriminator().HasValue("MultiEntityResult");
                });

            modelBuilder.Entity("F1Tipping.Model.Result", b =>
                {
                    b.HasOne("F1Tipping.Model.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_results_events_eventid");

                    b.HasOne("F1Tipping.Model.RacingEntity", "ResultHolder")
                        .WithMany()
                        .HasForeignKey("ResultHolderId")
                        .HasConstraintName("fk_results_racingentities_resultholderid");

                    b.Navigation("Event");

                    b.Navigation("ResultHolder");
                });

            modelBuilder.Entity("F1Tipping.Model.Round", b =>
                {
                    b.HasOne("F1Tipping.Model.Season", "Season")
                        .WithMany("Rounds")
                        .HasForeignKey("SeasonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_rounds_seasons_seasonid");

                    b.Navigation("Season");
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Player", b =>
                {
                    b.OwnsOne("F1Tipping.Model.Tipping.Player+Identity", "Details", b1 =>
                        {
                            b1.Property<Guid>("PlayerId")
                                .HasColumnType("uuid")
                                .HasColumnName("id");

                            b1.Property<string>("DisplayName")
                                .HasColumnType("text")
                                .HasColumnName("details_displayname");

                            b1.Property<string>("FirstName")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("details_firstname");

                            b1.Property<string>("LastName")
                                .HasColumnType("text")
                                .HasColumnName("details_lastname");

                            b1.HasKey("PlayerId");

                            b1.ToTable("players");

                            b1.WithOwner()
                                .HasForeignKey("PlayerId")
                                .HasConstraintName("fk_players_players_id");
                        });

                    b.Navigation("Details");
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Tip", b =>
                {
                    b.HasOne("F1Tipping.Model.RacingEntity", "Selection")
                        .WithMany()
                        .HasForeignKey("SelectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tips_racingentities_selectionid");

                    b.HasOne("F1Tipping.Model.Tipping.Player", "Tipper")
                        .WithMany()
                        .HasForeignKey("TipperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tips_players_tipperid");

                    b.HasOne("F1Tipping.Model.Result", "Target")
                        .WithMany()
                        .HasForeignKey("TargetEventId", "TargetType")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tips_results_targeteventid_targettype");

                    b.Navigation("Selection");

                    b.Navigation("Target");

                    b.Navigation("Tipper");
                });

            modelBuilder.Entity("MultiEntityResultRacingEntity", b =>
                {
                    b.HasOne("F1Tipping.Model.RacingEntity", null)
                        .WithMany()
                        .HasForeignKey("ResultHoldersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_multientityresultracingentity_racingentities_resultholdersid");

                    b.HasOne("F1Tipping.Model.MultiEntityResult", null)
                        .WithMany()
                        .HasForeignKey("MultiEntityResultEventId", "MultiEntityResultType")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_multientityresultracingentity_results_multientityresulteven~");
                });

            modelBuilder.Entity("F1Tipping.Model.Race", b =>
                {
                    b.HasOne("F1Tipping.Model.Round", "Weekend")
                        .WithMany("Events")
                        .HasForeignKey("WeekendId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_events_rounds_weekendid");

                    b.Navigation("Weekend");
                });

            modelBuilder.Entity("F1Tipping.Model.Driver", b =>
                {
                    b.HasOne("F1Tipping.Model.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_racingentities_racingentities_teamid");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("F1Tipping.Model.Round", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("F1Tipping.Model.Season", b =>
                {
                    b.Navigation("Rounds");
                });
#pragma warning restore 612, 618
        }
    }
}
