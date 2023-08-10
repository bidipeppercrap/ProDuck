﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProDuck.Models;

#nullable disable

namespace ProDuck.Migrations
{
    [DbContext(typeof(ProDuckContext))]
    [Migration("20230730151809_StockLocationNullable")]
    partial class StockLocationNullable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ProDuck.Models.Location", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long?>("LocationId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("ProDuck.Models.POSSession", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("ClosedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Note")
                        .HasColumnType("longtext");

                    b.Property<long>("POSId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("POSId");

                    b.HasIndex("UserId");

                    b.ToTable("POSSession");
                });

            modelBuilder.Entity("ProDuck.Models.PointOfSale", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("PointOfSale");
                });

            modelBuilder.Entity("ProDuck.Models.Product", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Barcode")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Cost")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Barcode")
                        .IsUnique();

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ProDuck.Models.Purchase", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Memo")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("SourceDocument")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long>("VendorId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("VendorId");

                    b.ToTable("Purchases");
                });

            modelBuilder.Entity("ProDuck.Models.PurchaseOrder", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("Cost")
                        .HasColumnType("int");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.Property<long>("PurchaseId")
                        .HasColumnType("bigint");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("PurchaseId");

                    b.ToTable("PurchaseOrders");
                });

            modelBuilder.Entity("ProDuck.Models.StockLocation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long?>("LocationId")
                        .HasColumnType("bigint");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.Property<int>("Stock")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.HasIndex("ProductId");

                    b.ToTable("StockLocation");
                });

            modelBuilder.Entity("ProDuck.Models.User", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ProDuck.Models.Vendor", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Contact")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Vendors");
                });

            modelBuilder.Entity("ProDuck.Models.Location", b =>
                {
                    b.HasOne("ProDuck.Models.Location", "ParentLocation")
                        .WithMany("ChildLocations")
                        .HasForeignKey("LocationId");

                    b.Navigation("ParentLocation");
                });

            modelBuilder.Entity("ProDuck.Models.POSSession", b =>
                {
                    b.HasOne("ProDuck.Models.PointOfSale", "POS")
                        .WithMany("Sessions")
                        .HasForeignKey("POSId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.User", "SessionOpener")
                        .WithMany("Sessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("POS");

                    b.Navigation("SessionOpener");
                });

            modelBuilder.Entity("ProDuck.Models.Purchase", b =>
                {
                    b.HasOne("ProDuck.Models.Vendor", "Vendor")
                        .WithMany("Purchases")
                        .HasForeignKey("VendorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Vendor");
                });

            modelBuilder.Entity("ProDuck.Models.PurchaseOrder", b =>
                {
                    b.HasOne("ProDuck.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.Purchase", "Purchase")
                        .WithMany("Orders")
                        .HasForeignKey("PurchaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Purchase");
                });

            modelBuilder.Entity("ProDuck.Models.StockLocation", b =>
                {
                    b.HasOne("ProDuck.Models.Location", "Location")
                        .WithMany("Products")
                        .HasForeignKey("LocationId");

                    b.HasOne("ProDuck.Models.Product", "Product")
                        .WithMany("Stocks")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Location");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ProDuck.Models.Location", b =>
                {
                    b.Navigation("ChildLocations");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("ProDuck.Models.PointOfSale", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("ProDuck.Models.Product", b =>
                {
                    b.Navigation("Stocks");
                });

            modelBuilder.Entity("ProDuck.Models.Purchase", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("ProDuck.Models.User", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("ProDuck.Models.Vendor", b =>
                {
                    b.Navigation("Purchases");
                });
#pragma warning restore 612, 618
        }
    }
}
