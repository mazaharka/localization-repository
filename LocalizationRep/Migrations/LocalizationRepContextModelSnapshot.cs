﻿// <auto-generated />
using System;
using LocalizationRep.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LocalizationRep.Migrations
{
    [DbContext(typeof(LocalizationRepContext))]
    partial class LocalizationRepContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("LocalizationRep.Models.FileModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.Property<string>("TypeOfLoad")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("FileModel");
                });

            modelBuilder.Entity("LocalizationRep.Models.LangKeyModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LangName")
                        .HasColumnType("TEXT");

                    b.Property<int?>("StyleJsonKeyModelID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("StyleJsonKeyModelID");

                    b.ToTable("LangKeyModel");
                });

            modelBuilder.Entity("LocalizationRep.Models.LangValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("LangKeyModelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Prular")
                        .HasColumnType("TEXT");

                    b.Property<string>("Single")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("LangKeyModelId")
                        .IsUnique();

                    b.ToTable("LangValue");
                });

            modelBuilder.Entity("LocalizationRep.Models.MainTable", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AndoridStringNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AndroidID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AndroidOnly")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CommonID")
                        .HasColumnType("TEXT");

                    b.Property<string>("IOsID")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IOsOnly")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFreezing")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SectionID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("SectionID");

                    b.ToTable("MainTable");
                });

            modelBuilder.Entity("LocalizationRep.Models.NotMatchedItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AndroidID")
                        .HasColumnType("TEXT");

                    b.Property<string>("CommentValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("CommonID")
                        .HasColumnType("TEXT");

                    b.Property<string>("NodeInnerText")
                        .HasColumnType("TEXT");

                    b.Property<int>("StringNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("NotMatchedItem");
                });

            modelBuilder.Entity("LocalizationRep.Models.Sections", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastIndexOfCommonID")
                        .HasColumnType("TEXT");

                    b.Property<string>("ShortName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Section");
                });

            modelBuilder.Entity("LocalizationRep.Models.StyleJsonKeyModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MainTablesID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StyleName")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("MainTablesID");

                    b.ToTable("StyleJsonKeyModel");
                });

            modelBuilder.Entity("LocalizationRep.Models.LangKeyModel", b =>
                {
                    b.HasOne("LocalizationRep.Models.StyleJsonKeyModel", "StyleJsonKeyModel")
                        .WithMany("LangKeyModels")
                        .HasForeignKey("StyleJsonKeyModelID");
                });

            modelBuilder.Entity("LocalizationRep.Models.LangValue", b =>
                {
                    b.HasOne("LocalizationRep.Models.LangKeyModel", "LangKeyModel")
                        .WithOne("LangValue")
                        .HasForeignKey("LocalizationRep.Models.LangValue", "LangKeyModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LocalizationRep.Models.MainTable", b =>
                {
                    b.HasOne("LocalizationRep.Models.Sections", "Section")
                        .WithMany("MainTables")
                        .HasForeignKey("SectionID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LocalizationRep.Models.StyleJsonKeyModel", b =>
                {
                    b.HasOne("LocalizationRep.Models.MainTable", "MainTables")
                        .WithMany("StyleJsonKeyModel")
                        .HasForeignKey("MainTablesID");
                });
#pragma warning restore 612, 618
        }
    }
}
