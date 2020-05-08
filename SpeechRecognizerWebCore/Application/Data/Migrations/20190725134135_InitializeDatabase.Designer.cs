﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpeechAndFaceRecognizerWebCore.Data;

namespace SpeechAndFaceRecognizerWebCore.Application.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20190725134135_InitializeDatabase")]
    partial class InitializeDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftFaceIdentificationPerson", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<Guid>("PersonGroupId");

                    b.Property<string>("UserData");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("PersonGroupId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("MicrosoftFaceIdentificationPersons");
                });

            modelBuilder.Entity("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftFaceIdentificationPersonGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("UserData");

                    b.HasKey("Id");

                    b.ToTable("MicrosoftFaceIdentificationPersonGroups");
                });

            modelBuilder.Entity("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftSpeekerIdentificationProfile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("NeedEnrollmentsCount");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("MicrosoftSpeekerIdentificationProfiles");
                });

            modelBuilder.Entity("SpeechAndFaceRecognizerWebCore.Data.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Login");

                    b.Property<string>("PasswordHash");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftFaceIdentificationPerson", b =>
                {
                    b.HasOne("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftFaceIdentificationPersonGroup", "PersonGroup")
                        .WithMany("Persons")
                        .HasForeignKey("PersonGroupId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SpeechAndFaceRecognizerWebCore.Data.Entities.User", "User")
                        .WithOne("MicrosoftFaceIdentificationPerson")
                        .HasForeignKey("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftFaceIdentificationPerson", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftSpeekerIdentificationProfile", b =>
                {
                    b.HasOne("SpeechAndFaceRecognizerWebCore.Data.Entities.User", "User")
                        .WithOne("MicrosoftSpeekerIdentificationProfile")
                        .HasForeignKey("SpeechAndFaceRecognizerWebCore.Data.Entities.MicrosoftSpeekerIdentificationProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
