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
    [Migration("20231002053229_DeliveredAtLandedCost")]
    partial class DeliveredAtLandedCost
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ClaimUser", b =>
                {
                    b.Property<long>("ClaimsId")
                        .HasColumnType("bigint");

                    b.Property<long>("UsersId")
                        .HasColumnType("bigint");

                    b.HasKey("ClaimsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("ClaimUser");
                });

            modelBuilder.Entity("PointOfSaleUser", b =>
                {
                    b.Property<long>("AssignedPOSesId")
                        .HasColumnType("bigint");

                    b.Property<long>("AssignedUsersId")
                        .HasColumnType("bigint");

                    b.HasKey("AssignedPOSesId", "AssignedUsersId");

                    b.HasIndex("AssignedUsersId");

                    b.ToTable("PointOfSaleUser");
                });

            modelBuilder.Entity("ProDuck.Models.Claim", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Claims");
                });

            modelBuilder.Entity("ProDuck.Models.Customer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("ProDuck.Models.CustomerPrice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long>("CustomerId")
                        .HasColumnType("bigint");

                    b.Property<int>("MinQty")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(65,30)");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ProductId");

                    b.ToTable("CustomerPrice");
                });

            modelBuilder.Entity("ProDuck.Models.LandedCost", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Biller")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<DateOnly?>("DeliveredAt")
                        .HasColumnType("date");

                    b.Property<bool>("IsDelivered")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsPurchase")
                        .HasColumnType("tinyint(1)");

                    b.Property<long?>("SourceLocationId")
                        .HasColumnType("bigint");

                    b.Property<long?>("TargetLocationId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("SourceLocationId");

                    b.HasIndex("TargetLocationId");

                    b.ToTable("LandedCosts");
                });

            modelBuilder.Entity("ProDuck.Models.LandedCostItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(65,30)");

                    b.Property<long?>("LandedCostId")
                        .HasColumnType("bigint");

                    b.Property<long?>("LandedCostItemId")
                        .HasColumnType("bigint");

                    b.Property<long>("PurchaseOrderId")
                        .HasColumnType("bigint");

                    b.Property<int>("Qty")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LandedCostId");

                    b.HasIndex("LandedCostItemId");

                    b.HasIndex("PurchaseOrderId");

                    b.ToTable("LandedCostItems");
                });

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

            modelBuilder.Entity("ProDuck.Models.Order", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("CustomerId")
                        .HasColumnType("bigint");

                    b.Property<long>("POSSessionId")
                        .HasColumnType("bigint");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("POSSessionId");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("ProDuck.Models.OrderItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(65,30)");

                    b.Property<long>("OrderId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(65,30)");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.Property<int>("Qty")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrdersItem");
                });

            modelBuilder.Entity("ProDuck.Models.POSSession", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ClosedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("ClosingBalance")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("ClosingRemark")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("OpenedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("OpeningBalance")
                        .HasColumnType("decimal(65,30)");

                    b.Property<long>("POSId")
                        .HasColumnType("bigint");

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

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

                    b.Property<long?>("CategoryId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(65,30)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(65,30)");

                    b.HasKey("Id");

                    b.HasIndex("Barcode")
                        .IsUnique();

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ProDuck.Models.ProductCategory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long?>("ProductCategoryId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ProductCategoryId");

                    b.ToTable("ProductCategories");
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

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

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

            modelBuilder.Entity("ClaimUser", b =>
                {
                    b.HasOne("ProDuck.Models.Claim", null)
                        .WithMany()
                        .HasForeignKey("ClaimsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PointOfSaleUser", b =>
                {
                    b.HasOne("ProDuck.Models.PointOfSale", null)
                        .WithMany()
                        .HasForeignKey("AssignedPOSesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.User", null)
                        .WithMany()
                        .HasForeignKey("AssignedUsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ProDuck.Models.CustomerPrice", b =>
                {
                    b.HasOne("ProDuck.Models.Customer", "Customer")
                        .WithMany("ProductPrices")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.Product", "Product")
                        .WithMany("CustomerPrices")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ProDuck.Models.LandedCost", b =>
                {
                    b.HasOne("ProDuck.Models.Location", "SourceLocation")
                        .WithMany()
                        .HasForeignKey("SourceLocationId");

                    b.HasOne("ProDuck.Models.Location", "TargetLocation")
                        .WithMany()
                        .HasForeignKey("TargetLocationId");

                    b.Navigation("SourceLocation");

                    b.Navigation("TargetLocation");
                });

            modelBuilder.Entity("ProDuck.Models.LandedCostItem", b =>
                {
                    b.HasOne("ProDuck.Models.LandedCost", "LandedCost")
                        .WithMany("Items")
                        .HasForeignKey("LandedCostId");

                    b.HasOne("ProDuck.Models.LandedCostItem", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("LandedCostItemId");

                    b.HasOne("ProDuck.Models.PurchaseOrder", "PurchaseOrder")
                        .WithMany("LandedCostItems")
                        .HasForeignKey("PurchaseOrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LandedCost");

                    b.Navigation("Parent");

                    b.Navigation("PurchaseOrder");
                });

            modelBuilder.Entity("ProDuck.Models.Location", b =>
                {
                    b.HasOne("ProDuck.Models.Location", "ParentLocation")
                        .WithMany("ChildLocations")
                        .HasForeignKey("LocationId");

                    b.Navigation("ParentLocation");
                });

            modelBuilder.Entity("ProDuck.Models.Order", b =>
                {
                    b.HasOne("ProDuck.Models.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId");

                    b.HasOne("ProDuck.Models.POSSession", "POSSession")
                        .WithMany("Orders")
                        .HasForeignKey("POSSessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.User", "ServedBy")
                        .WithMany("OrdersServed")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("POSSession");

                    b.Navigation("ServedBy");
                });

            modelBuilder.Entity("ProDuck.Models.OrderItem", b =>
                {
                    b.HasOne("ProDuck.Models.Order", "Order")
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProDuck.Models.Product", "Product")
                        .WithMany("OrderItems")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
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

            modelBuilder.Entity("ProDuck.Models.Product", b =>
                {
                    b.HasOne("ProDuck.Models.ProductCategory", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("ProDuck.Models.ProductCategory", b =>
                {
                    b.HasOne("ProDuck.Models.ProductCategory", "ParentCategory")
                        .WithMany("ChildCategories")
                        .HasForeignKey("ProductCategoryId");

                    b.Navigation("ParentCategory");
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
                        .WithMany("PurchaseOrders")
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

            modelBuilder.Entity("ProDuck.Models.Customer", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("ProductPrices");
                });

            modelBuilder.Entity("ProDuck.Models.LandedCost", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("ProDuck.Models.LandedCostItem", b =>
                {
                    b.Navigation("Children");
                });

            modelBuilder.Entity("ProDuck.Models.Location", b =>
                {
                    b.Navigation("ChildLocations");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("ProDuck.Models.Order", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("ProDuck.Models.POSSession", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("ProDuck.Models.PointOfSale", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("ProDuck.Models.Product", b =>
                {
                    b.Navigation("CustomerPrices");

                    b.Navigation("OrderItems");

                    b.Navigation("PurchaseOrders");

                    b.Navigation("Stocks");
                });

            modelBuilder.Entity("ProDuck.Models.ProductCategory", b =>
                {
                    b.Navigation("ChildCategories");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("ProDuck.Models.Purchase", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("ProDuck.Models.PurchaseOrder", b =>
                {
                    b.Navigation("LandedCostItems");
                });

            modelBuilder.Entity("ProDuck.Models.User", b =>
                {
                    b.Navigation("OrdersServed");

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
