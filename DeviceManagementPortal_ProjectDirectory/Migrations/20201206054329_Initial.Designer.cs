﻿// <auto-generated />
using System;
using DeviceManagementPortal.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DeviceManagementPortal.Migrations
{
    [DbContext(typeof(DeviceManagementDbContext))]
    [Migration("20201206054329_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("DeviceManagementPortal.Models.DomainModels.Backend", b =>
                {
                    b.Property<int>("BackendID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("BackendID");

                    b.ToTable("Backends");
                });

            modelBuilder.Entity("DeviceManagementPortal.Models.DomainModels.Device", b =>
                {
                    b.Property<int>("DeviceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Enabled")
                        .HasColumnType("bit");

                    b.Property<string>("IMEI")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<long>("SimCardNumber")
                        .HasColumnType("bigint");

                    b.HasKey("DeviceID");

                    b.HasIndex("IMEI")
                        .IsUnique();

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("DeviceManagementPortal.Models.DomainModels.DeviceBackend", b =>
                {
                    b.Property<int>("DeviceBackendID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("BackendID")
                        .HasColumnType("int");

                    b.Property<int>("DeviceID")
                        .HasColumnType("int");

                    b.HasKey("DeviceBackendID");

                    b.HasIndex("BackendID");

                    b.HasIndex("DeviceID");

                    b.ToTable("DeviceBackends");
                });

            modelBuilder.Entity("DeviceManagementPortal.Models.DomainModels.DeviceBackend", b =>
                {
                    b.HasOne("DeviceManagementPortal.Models.DomainModels.Backend", "Backend")
                        .WithMany("DeviceBackends")
                        .HasForeignKey("BackendID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceManagementPortal.Models.DomainModels.Device", "Device")
                        .WithMany("DeviceBackends")
                        .HasForeignKey("DeviceID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Backend");

                    b.Navigation("Device");
                });

            modelBuilder.Entity("DeviceManagementPortal.Models.DomainModels.Backend", b =>
                {
                    b.Navigation("DeviceBackends");
                });

            modelBuilder.Entity("DeviceManagementPortal.Models.DomainModels.Device", b =>
                {
                    b.Navigation("DeviceBackends");
                });
#pragma warning restore 612, 618
        }
    }
}
