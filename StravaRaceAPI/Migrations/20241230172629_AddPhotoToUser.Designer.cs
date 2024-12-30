﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StravaRaceAPI.Entities;

#nullable disable

namespace StravaRaceAPI.Migrations
{
    [DbContext(typeof(ApiDBContext))]
    [Migration("20241230172629_AddPhotoToUser")]
    partial class AddPhotoToUser
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.20")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("StravaRaceAPI.Entities.Event", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("CreationDate")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime(0)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("EndDate")
                        .HasPrecision(0)
                        .HasColumnType("datetime(0)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartDate")
                        .HasPrecision(0)
                        .HasColumnType("datetime(0)");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.RaceSegment", b =>
                {
                    b.Property<ulong>("EventId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("SegmentId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("EventId", "SegmentId");

                    b.HasIndex("SegmentId");

                    b.ToTable("RaceSegment");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Result", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("EventId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("SegmentId")
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("Time")
                        .HasColumnType("int unsigned");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("SegmentId");

                    b.HasIndex("UserId");

                    b.ToTable("Results");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Segment", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<float>("Distance")
                        .HasColumnType("float");

                    b.Property<float>("Elevation")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Segments");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Token", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ExpirationOfToken")
                        .HasPrecision(0)
                        .HasColumnType("datetime(0)");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("UserId");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ProfilePictureUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.UserWithEvent", b =>
                {
                    b.Property<ulong>("EventId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("EventId", "UserId");

                    b.HasAlternateKey("UserId", "EventId");

                    b.ToTable("UsersWithEvents");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Event", b =>
                {
                    b.HasOne("StravaRaceAPI.Entities.User", "Creator")
                        .WithMany("CreatedEvents")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.RaceSegment", b =>
                {
                    b.HasOne("StravaRaceAPI.Entities.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StravaRaceAPI.Entities.Segment", "Segment")
                        .WithMany()
                        .HasForeignKey("SegmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Segment");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Result", b =>
                {
                    b.HasOne("StravaRaceAPI.Entities.Event", "Event")
                        .WithMany("Results")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StravaRaceAPI.Entities.Segment", "Segment")
                        .WithMany("Results")
                        .HasForeignKey("SegmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StravaRaceAPI.Entities.User", "User")
                        .WithMany("Results")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Segment");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Token", b =>
                {
                    b.HasOne("StravaRaceAPI.Entities.User", "User")
                        .WithOne("Token")
                        .HasForeignKey("StravaRaceAPI.Entities.Token", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.UserWithEvent", b =>
                {
                    b.HasOne("StravaRaceAPI.Entities.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StravaRaceAPI.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Event", b =>
                {
                    b.Navigation("Results");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.Segment", b =>
                {
                    b.Navigation("Results");
                });

            modelBuilder.Entity("StravaRaceAPI.Entities.User", b =>
                {
                    b.Navigation("CreatedEvents");

                    b.Navigation("Results");

                    b.Navigation("Token")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
