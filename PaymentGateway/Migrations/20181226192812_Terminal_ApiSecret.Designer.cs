﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentGateway.Data;

namespace PaymentGateway.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20181226192812_Terminal_ApiSecret")]
    partial class Terminal_ApiSecret
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("PaymentGateway.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("AspNetRoleId");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("PaymentGateway.Models.AppParam", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("varchar(1000)");

                    b.HasKey("Id");

                    b.ToTable("AppParam");
                });

            modelBuilder.Entity("PaymentGateway.Models.Cryptocurrency", b =>
                {
                    b.Property<string>("Code")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Code");

                    b.ToTable("Cryptocurrency");
                });

            modelBuilder.Entity("PaymentGateway.Models.Currency", b =>
                {
                    b.Property<string>("Code")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(3)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Code");

                    b.ToTable("Currency");
                });

            modelBuilder.Entity("PaymentGateway.Models.Merchant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<sbyte>("Status");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.Property<string>("WalletAddress")
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Merchant");
                });

            modelBuilder.Entity("PaymentGateway.Models.Payment", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(64)");

                    b.Property<int>("BlockNumber");

                    b.Property<string>("CallbackUrl");

                    b.Property<string>("CancelUrl");

                    b.Property<string>("CompleteUrl");

                    b.Property<decimal>("ExchangeBrokerFee");

                    b.Property<string>("ExternalOrderId");

                    b.Property<decimal>("GraftToSaleRate");

                    b.Property<decimal>("MerchantAmount");

                    b.Property<decimal>("PayAmount");

                    b.Property<string>("PayCurrency")
                        .IsRequired()
                        .HasColumnType("char(6)");

                    b.Property<decimal>("PayToSaleRate");

                    b.Property<string>("PayWalletAddress")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<decimal>("SaleAmount");

                    b.Property<string>("SaleCurrency")
                        .IsRequired()
                        .HasColumnType("char(3)");

                    b.Property<string>("SaleDetails")
                        .HasColumnType("varchar(500)");

                    b.Property<decimal>("ServiceProviderFee");

                    b.Property<int>("ServiceProviderId");

                    b.Property<sbyte>("Status");

                    b.Property<int>("StoreId");

                    b.Property<int>("TerminalId");

                    b.Property<DateTime>("TransactionDate");

                    b.HasKey("Id");

                    b.HasIndex("ServiceProviderId");

                    b.HasIndex("StoreId");

                    b.HasIndex("TerminalId");

                    b.ToTable("Payment");
                });

            modelBuilder.Entity("PaymentGateway.Models.ServiceProvider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<sbyte>("Status");

                    b.Property<float>("TransactionFee");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.Property<string>("WalletAddress")
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ServiceProvider");
                });

            modelBuilder.Entity("PaymentGateway.Models.StimulusTransaction", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<int>("BlockNumber");

                    b.Property<string>("Error");

                    b.Property<string>("PayWalletAddress")
                        .HasColumnType("varchar(200)");

                    b.Property<string>("RecvWalletAddress")
                        .HasColumnType("varchar(200)");

                    b.Property<sbyte>("Status");

                    b.Property<DateTime>("TransactionDate");

                    b.HasKey("Id");

                    b.ToTable("StimulusTransaction");
                });

            modelBuilder.Entity("PaymentGateway.Models.Store", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .HasColumnType("varchar(200)");

                    b.Property<int>("MerchantId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<sbyte>("Status")
                        .HasColumnName("Status");

                    b.HasKey("Id");

                    b.HasIndex("MerchantId");

                    b.ToTable("Store");
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagMerchant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("TagMerchant");
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagMerchantConnection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("MerchantId");

                    b.Property<int>("TagId");

                    b.HasKey("Id");

                    b.HasIndex("MerchantId");

                    b.HasIndex("TagId");

                    b.ToTable("TagMerchantConnection");
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagTerminal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("TagTerminals");
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagTerminalConnection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("TagId");

                    b.Property<int>("TerminalId");

                    b.HasKey("Id");

                    b.HasIndex("TagId");

                    b.HasIndex("TerminalId");

                    b.ToTable("TagTerminalConnections");
                });

            modelBuilder.Entity("PaymentGateway.Models.Terminal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApiSecret");

                    b.Property<int>("MerchantId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("SerialNumber")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<int>("ServiceProviderId");

                    b.Property<sbyte>("Status");

                    b.Property<int>("StoreId");

                    b.Property<bool>("Virtual");

                    b.HasKey("Id");

                    b.HasIndex("MerchantId");

                    b.HasIndex("ServiceProviderId");

                    b.HasIndex("StoreId");

                    b.ToTable("Terminal");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Merchant", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Payment", b =>
                {
                    b.HasOne("PaymentGateway.Models.ServiceProvider", "ServiceProvider")
                        .WithMany()
                        .HasForeignKey("ServiceProviderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Models.Store", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Models.Terminal", "Terminal")
                        .WithMany()
                        .HasForeignKey("TerminalId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.ServiceProvider", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Store", b =>
                {
                    b.HasOne("PaymentGateway.Models.Merchant", "Merchant")
                        .WithMany("Stores")
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagMerchant", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagMerchantConnection", b =>
                {
                    b.HasOne("PaymentGateway.Models.Merchant", "Merchant")
                        .WithMany("Tags")
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Models.Tags.TagMerchant", "Tag")
                        .WithMany("TagMerchantConnections")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagTerminal", b =>
                {
                    b.HasOne("PaymentGateway.Data.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Tags.TagTerminalConnection", b =>
                {
                    b.HasOne("PaymentGateway.Models.Tags.TagTerminal", "Tag")
                        .WithMany("TagTerminalConnections")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Models.Terminal", "Terminal")
                        .WithMany("Tags")
                        .HasForeignKey("TerminalId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaymentGateway.Models.Terminal", b =>
                {
                    b.HasOne("PaymentGateway.Models.Merchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Models.ServiceProvider", "ServiceProvider")
                        .WithMany()
                        .HasForeignKey("ServiceProviderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaymentGateway.Models.Store", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
