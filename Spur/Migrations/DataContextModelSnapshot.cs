﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Spur.Data;

#nullable disable

namespace Spur.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.4");

            modelBuilder.Entity("AthleteChallenge", b =>
                {
                    b.Property<int>("AthletesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChallengesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AthletesId", "ChallengesId");

                    b.HasIndex("ChallengesId");

                    b.ToTable("AthleteChallenge");
                });

            modelBuilder.Entity("Spur.Model.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AthleteId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DetailsId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("StravaId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AthleteId");

                    b.HasIndex("DetailsId");

                    b.ToTable("Activities");
                });

            modelBuilder.Entity("Spur.Model.ActivityDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ActivityDetails");
                });

            modelBuilder.Entity("Spur.Model.Athlete", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("AccessTokenExpiry")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImgUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("StravaId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Athletes");
                });

            modelBuilder.Entity("Spur.Model.Challenge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("End")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Start")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Challenges");
                });

            modelBuilder.Entity("AthleteChallenge", b =>
                {
                    b.HasOne("Spur.Model.Athlete", null)
                        .WithMany()
                        .HasForeignKey("AthletesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Spur.Model.Challenge", null)
                        .WithMany()
                        .HasForeignKey("ChallengesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Spur.Model.Activity", b =>
                {
                    b.HasOne("Spur.Model.Athlete", "Athlete")
                        .WithMany("Activities")
                        .HasForeignKey("AthleteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Spur.Model.ActivityDetails", "Details")
                        .WithMany()
                        .HasForeignKey("DetailsId");

                    b.Navigation("Athlete");

                    b.Navigation("Details");
                });

            modelBuilder.Entity("Spur.Model.Athlete", b =>
                {
                    b.Navigation("Activities");
                });
#pragma warning restore 612, 618
        }
    }
}
