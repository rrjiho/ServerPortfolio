﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.DB;

#nullable disable

namespace Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Server.DB.AccountDb", b =>
                {
                    b.Property<int>("AccountDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AccountDbId"));

                    b.Property<string>("AccountName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AccountDbId");

                    b.HasIndex("AccountName")
                        .IsUnique()
                        .HasFilter("[AccountName] IS NOT NULL");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Server.DB.ItemDb", b =>
                {
                    b.Property<int>("ItemDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ItemDbId"));

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<bool>("Equipped")
                        .HasColumnType("bit");

                    b.Property<int?>("OwnerDbId")
                        .HasColumnType("int");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<int>("Slot")
                        .HasColumnType("int");

                    b.Property<int>("TemplateId")
                        .HasColumnType("int");

                    b.HasKey("ItemDbId");

                    b.HasIndex("OwnerDbId");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.Property<int>("PlayerDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlayerDbId"));

                    b.Property<int>("AccountDbId")
                        .HasColumnType("int");

                    b.Property<int>("Attack")
                        .HasColumnType("int");

                    b.Property<int>("Gold")
                        .HasColumnType("int");

                    b.Property<int>("Hp")
                        .HasColumnType("int");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("MaxHp")
                        .HasColumnType("int");

                    b.Property<string>("PlayerName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<float>("Speed")
                        .HasColumnType("real");

                    b.Property<int>("TotalExp")
                        .HasColumnType("int");

                    b.HasKey("PlayerDbId");

                    b.HasIndex("AccountDbId");

                    b.HasIndex("PlayerName")
                        .IsUnique()
                        .HasFilter("[PlayerName] IS NOT NULL");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("Server.DB.PostDb", b =>
                {
                    b.Property<int>("PostDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PostDbId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PlayerDbId")
                        .HasColumnType("int");

                    b.Property<string>("PlayerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PostNumber")
                        .HasColumnType("int");

                    b.Property<string>("SendPlayerName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PostDbId");

                    b.HasIndex("PlayerDbId", "PostNumber");

                    b.ToTable("Post");
                });

            modelBuilder.Entity("Server.DB.QuestDb", b =>
                {
                    b.Property<int>("QuestDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("QuestDbId"));

                    b.Property<bool>("IsAccepted")
                        .HasColumnType("bit");

                    b.Property<int>("KillCount")
                        .HasColumnType("int");

                    b.Property<int>("PlayerDbId")
                        .HasColumnType("int");

                    b.Property<int>("QuestNumber")
                        .HasColumnType("int");

                    b.HasKey("QuestDbId");

                    b.HasIndex("PlayerDbId");

                    b.ToTable("Quest");
                });

            modelBuilder.Entity("Server.DB.ItemDb", b =>
                {
                    b.HasOne("Server.DB.PlayerDb", "Owner")
                        .WithMany("Items")
                        .HasForeignKey("OwnerDbId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.HasOne("Server.DB.AccountDb", "Account")
                        .WithMany("Players")
                        .HasForeignKey("AccountDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Server.DB.PostDb", b =>
                {
                    b.HasOne("Server.DB.PlayerDb", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Server.DB.QuestDb", b =>
                {
                    b.HasOne("Server.DB.PlayerDb", "Player")
                        .WithMany("Quests")
                        .HasForeignKey("PlayerDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Server.DB.AccountDb", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.Navigation("Items");

                    b.Navigation("Quests");
                });
#pragma warning restore 612, 618
        }
    }
}
