﻿// <auto-generated />
using System;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace F1Tipping.Data.ModelMigrations
{
    [DbContext(typeof(ModelDbContext))]
    [Migration("20250226053302_AddDetailsToDrivers")]
    partial class AddDetailsToDrivers
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("F1Tipping.Model.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.HasKey("Id");

                    b.ToTable("Events");

                    b.HasDiscriminator().HasValue("Event");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("F1Tipping.Model.RacingEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.HasKey("Id");

                    b.ToTable("RacingEntities");

                    b.HasDiscriminator().HasValue("RacingEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("F1Tipping.Model.Result", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<Guid?>("ResultHolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Set")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("SetByAuthUser")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("EventId", "Type");

                    b.HasIndex("ResultHolderId");

                    b.ToTable("Results");
                });

            modelBuilder.Entity("F1Tipping.Model.Round", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<Guid>("SeasonId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SeasonId");

                    b.ToTable("Rounds");
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.PrimitiveCollection<string>("AdditionalAuthedUsers")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("AuthUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Tip", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SelectionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("SubmittedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("SubmittedBy_AuthUser")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TargetEventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TargetType")
                        .HasColumnType("int");

                    b.Property<Guid>("TipperId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("SelectionId");

                    b.HasIndex("TipperId");

                    b.HasIndex("TargetEventId", "TargetType");

                    b.ToTable("Tips");
                });

            modelBuilder.Entity("F1Tipping.Model.Race", b =>
                {
                    b.HasBaseType("F1Tipping.Model.Event");

                    b.Property<DateTimeOffset>("QualificationStart")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("RaceStart")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<Guid>("WeekendId")
                        .HasColumnType("uniqueidentifier");

                    b.HasIndex("WeekendId");

                    b.HasDiscriminator().HasValue("Race");
                });

            modelBuilder.Entity("F1Tipping.Model.Season", b =>
                {
                    b.HasBaseType("F1Tipping.Model.Event");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasDiscriminator().HasValue("Season");
                });

            modelBuilder.Entity("F1Tipping.Model.Driver", b =>
                {
                    b.HasBaseType("F1Tipping.Model.RacingEntity");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nationality")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uniqueidentifier");

                    b.HasIndex("TeamId");

                    b.HasDiscriminator().HasValue("Driver");
                });

            modelBuilder.Entity("F1Tipping.Model.Team", b =>
                {
                    b.HasBaseType("F1Tipping.Model.RacingEntity");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("Team");
                });

            modelBuilder.Entity("F1Tipping.Model.Result", b =>
                {
                    b.HasOne("F1Tipping.Model.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("F1Tipping.Model.RacingEntity", "ResultHolder")
                        .WithMany()
                        .HasForeignKey("ResultHolderId");

                    b.Navigation("Event");

                    b.Navigation("ResultHolder");
                });

            modelBuilder.Entity("F1Tipping.Model.Round", b =>
                {
                    b.HasOne("F1Tipping.Model.Season", "Season")
                        .WithMany("Rounds")
                        .HasForeignKey("SeasonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Season");
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Player", b =>
                {
                    b.OwnsOne("F1Tipping.Model.Tipping.Player+Identity", "Details", b1 =>
                        {
                            b1.Property<Guid>("PlayerId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("DisplayName")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("FirstName")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("LastName")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("PlayerId");

                            b1.ToTable("Players");

                            b1.WithOwner()
                                .HasForeignKey("PlayerId");
                        });

                    b.Navigation("Details");
                });

            modelBuilder.Entity("F1Tipping.Model.Tipping.Tip", b =>
                {
                    b.HasOne("F1Tipping.Model.RacingEntity", "Selection")
                        .WithMany()
                        .HasForeignKey("SelectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("F1Tipping.Model.Tipping.Player", "Tipper")
                        .WithMany()
                        .HasForeignKey("TipperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("F1Tipping.Model.Result", "Target")
                        .WithMany()
                        .HasForeignKey("TargetEventId", "TargetType")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Selection");

                    b.Navigation("Target");

                    b.Navigation("Tipper");
                });

            modelBuilder.Entity("F1Tipping.Model.Race", b =>
                {
                    b.HasOne("F1Tipping.Model.Round", "Weekend")
                        .WithMany()
                        .HasForeignKey("WeekendId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Weekend");
                });

            modelBuilder.Entity("F1Tipping.Model.Driver", b =>
                {
                    b.HasOne("F1Tipping.Model.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("F1Tipping.Model.Season", b =>
                {
                    b.Navigation("Rounds");
                });
#pragma warning restore 612, 618
        }
    }
}
