﻿// <auto-generated />
using LocalizationRep.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LocalizationRep.Migrations
{
    [DbContext(typeof(LocalizationRepContext))]
    [Migration("20200526145736_AddLastIndexOfCommonID")]
    partial class AddLastIndexOfCommonID
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("LocalizationRep.Models.FileModel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("FileModel");
                });

            modelBuilder.Entity("LocalizationRep.Models.MainTable", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AndroidID")
                        .HasColumnType("TEXT");

                    b.Property<string>("CommonID")
                        .HasColumnType("TEXT");

                    b.Property<string>("IOsID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFreezing")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SectionID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TextEN")
                        .HasColumnType("TEXT");

                    b.Property<string>("TextRU")
                        .HasColumnType("TEXT");

                    b.Property<string>("TextUA")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("SectionID");

                    b.ToTable("MainTable");
                });

            modelBuilder.Entity("LocalizationRep.Models.Sections", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ShortName")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastIndexOfCommonID")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Section");
                });

            modelBuilder.Entity("LocalizationRep.Models.MainTable", b =>
                {
                    b.HasOne("LocalizationRep.Models.Sections", "Section")
                        .WithMany("MainTables")
                        .HasForeignKey("SectionID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}