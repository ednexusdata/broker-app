﻿// <auto-generated />
using System;
using EdNexusData.Broker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.PostgreSql
{
    [DbContext(typeof(PostgresDbContext))]
    partial class PostgresDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EdNexusData.Broker.Data.Seed", b =>
                {
                    b.Property<string>("SeedId")
                        .HasColumnType("text");

                    b.Property<string>("SeedName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("SeedId");

                    b.ToTable("__BrokerSeedsHistory", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("EducationOrganizationId");

                    b.Property<string>("Address")
                        .HasColumnType("jsonb");

                    b.Property<string>("Contacts")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Domain")
                        .HasColumnType("text");

                    b.Property<int>("EducationOrganizationType")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .HasColumnType("text");

                    b.Property<Guid?>("ParentOrganizationId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Domain")
                        .IsUnique();

                    b.HasIndex("ParentOrganizationId");

                    b.ToTable("EducationOrganizations");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganizationConnectorSettings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("EducationOrganizationConnectorSettingsId");

                    b.Property<string>("Connector")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("EducationOrganizationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Settings")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("EducationOrganizationId", "Connector")
                        .IsUnique();

                    b.ToTable("EducationOrganizationConnectorSettings");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganizationPayloadSettings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("EducationOrganizationPayloadSettingsId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("EducationOrganizationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("EducationOrganizationId", "Payload")
                        .IsUnique();

                    b.ToTable("EducationOrganizationPayloadSettings");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Mapping", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("MappingId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("JsonDestinationMapping")
                        .HasColumnType("jsonb");

                    b.Property<string>("JsonSourceMapping")
                        .HasColumnType("jsonb");

                    b.Property<string>("MappingType")
                        .HasColumnType("text");

                    b.Property<string>("OriginalSchema")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("PayloadContentActionId")
                        .HasColumnType("uuid");

                    b.Property<string>("StudentAttributes")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.Property<byte>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint")
                        .HasDefaultValue((byte)1);

                    b.HasKey("Id");

                    b.HasIndex("PayloadContentActionId", "Version")
                        .IsUnique();

                    b.ToTable("Mappings", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("MessageId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("MessageContents")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("MessageTimestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<int>("RequestResponse")
                        .HasColumnType("integer");

                    b.Property<string>("TransmissionDetails")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RequestId");

                    b.ToTable("Messages", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.PayloadContent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("PayloadContentId");

                    b.Property<byte[]>("BlobContent")
                        .HasColumnType("bytea");

                    b.Property<string>("ContentType")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.Property<string>("JsonContent")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("XmlContent")
                        .HasColumnType("xml");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.HasIndex("RequestId");

                    b.ToTable("PayloadContents", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.PayloadContentAction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("PayloadContentActionId");

                    b.Property<Guid?>("ActiveMappingId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("PayloadContentActionType")
                        .HasColumnType("text");

                    b.Property<Guid?>("PayloadContentId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Process")
                        .HasColumnType("boolean");

                    b.Property<string>("Settings")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ActiveMappingId")
                        .IsUnique();

                    b.HasIndex("PayloadContentId", "PayloadContentActionType")
                        .IsUnique();

                    b.ToTable("PayloadContentActions", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Request", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("RequestId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EducationOrganizationId")
                        .HasColumnType("uuid");

                    b.Property<int>("IncomingOutgoing")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("InitialRequestSentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ProcessState")
                        .HasColumnType("text");

                    b.Property<string>("RequestManifest")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("RequestProcessUserId")
                        .HasColumnType("uuid");

                    b.Property<int>("RequestStatus")
                        .HasColumnType("integer");

                    b.Property<string>("ResponseManifest")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("ResponseProcessUserId")
                        .HasColumnType("uuid");

                    b.Property<string>("Student")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("WorkerInstance")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EducationOrganizationId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("UserId");

                    b.Property<int>("AllEducationOrganizations")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasAnnotation("Relational:IsStored", true);

                    b.Property<Guid?>("CreatedBy")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("uuid")
                        .HasAnnotation("Relational:IsStored", true);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsSuperAdmin")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.UserRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("UserRoleId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("EducationOrganizationId")
                        .HasColumnType("uuid");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("EducationOrganizationId", "UserId")
                        .IsUnique();

                    b.ToTable("UserRoles", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Worker.Job", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("JobId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("FinishDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("JobParameters")
                        .HasColumnType("jsonb");

                    b.Property<int>("JobStatus")
                        .HasColumnType("integer");

                    b.Property<string>("JobType")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("QueueDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("ReferenceGuid")
                        .HasColumnType("uuid");

                    b.Property<string>("ReferenceType")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("StartDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("WorkerInstance")
                        .HasColumnType("text");

                    b.Property<string>("WorkerState")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Worker_Jobs", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganization", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.EducationOrganization", "ParentOrganization")
                        .WithMany("EducationOrganizations")
                        .HasForeignKey("ParentOrganizationId");

                    b.Navigation("ParentOrganization");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganizationConnectorSettings", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.EducationOrganization", "EducationOrganization")
                        .WithMany()
                        .HasForeignKey("EducationOrganizationId");

                    b.Navigation("EducationOrganization");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganizationPayloadSettings", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.EducationOrganization", "EducationOrganization")
                        .WithMany()
                        .HasForeignKey("EducationOrganizationId");

                    b.OwnsOne("EdNexusData.Broker.Domain.IncomingPayloadSettings", "IncomingPayloadSettings", b1 =>
                        {
                            b1.Property<Guid>("EducationOrganizationPayloadSettingsId")
                                .HasColumnType("uuid");

                            b1.Property<string>("StudentInformationSystem")
                                .HasColumnType("text");

                            b1.HasKey("EducationOrganizationPayloadSettingsId");

                            b1.ToTable("EducationOrganizationPayloadSettings");

                            b1.ToJson("IncomingPayloadSettings");

                            b1.WithOwner()
                                .HasForeignKey("EducationOrganizationPayloadSettingsId");

                            b1.OwnsMany("EdNexusData.Broker.Domain.PayloadSettingsContentType", "PayloadContents", b2 =>
                                {
                                    b2.Property<Guid>("IncomingPayloadSettingsEducationOrganizationPayloadSettingsId")
                                        .HasColumnType("uuid");

                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("integer");

                                    b2.Property<Guid>("JobId")
                                        .HasColumnType("uuid");

                                    b2.Property<string>("PayloadContentType")
                                        .IsRequired()
                                        .HasColumnType("text");

                                    b2.Property<string>("Settings")
                                        .HasColumnType("text");

                                    b2.HasKey("IncomingPayloadSettingsEducationOrganizationPayloadSettingsId", "Id");

                                    b2.ToTable("EducationOrganizationPayloadSettings");

                                    b2.WithOwner()
                                        .HasForeignKey("IncomingPayloadSettingsEducationOrganizationPayloadSettingsId");
                                });

                            b1.Navigation("PayloadContents");
                        });

                    b.OwnsOne("EdNexusData.Broker.Domain.OutgoingPayloadSettings", "OutgoingPayloadSettings", b1 =>
                        {
                            b1.Property<Guid>("EducationOrganizationPayloadSettingsId")
                                .HasColumnType("uuid");

                            b1.Property<string>("StudentLookupConnector")
                                .HasColumnType("text");

                            b1.HasKey("EducationOrganizationPayloadSettingsId");

                            b1.ToTable("EducationOrganizationPayloadSettings");

                            b1.ToJson("OutgoingPayloadSettings");

                            b1.WithOwner()
                                .HasForeignKey("EducationOrganizationPayloadSettingsId");

                            b1.OwnsMany("EdNexusData.Broker.Domain.PayloadSettingsContentType", "PayloadContents", b2 =>
                                {
                                    b2.Property<Guid>("OutgoingPayloadSettingsEducationOrganizationPayloadSettingsId")
                                        .HasColumnType("uuid");

                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("integer");

                                    b2.Property<Guid>("JobId")
                                        .HasColumnType("uuid");

                                    b2.Property<string>("PayloadContentType")
                                        .IsRequired()
                                        .HasColumnType("text");

                                    b2.Property<string>("Settings")
                                        .HasColumnType("text");

                                    b2.HasKey("OutgoingPayloadSettingsEducationOrganizationPayloadSettingsId", "Id");

                                    b2.ToTable("EducationOrganizationPayloadSettings");

                                    b2.WithOwner()
                                        .HasForeignKey("OutgoingPayloadSettingsEducationOrganizationPayloadSettingsId");
                                });

                            b1.Navigation("PayloadContents");
                        });

                    b.Navigation("EducationOrganization");

                    b.Navigation("IncomingPayloadSettings");

                    b.Navigation("OutgoingPayloadSettings");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Mapping", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.PayloadContentAction", "PayloadContentAction")
                        .WithMany("Mappings")
                        .HasForeignKey("PayloadContentActionId");

                    b.Navigation("PayloadContentAction");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Message", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.Request", "Request")
                        .WithMany("Messages")
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Request");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.PayloadContent", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.Message", "Message")
                        .WithMany("PayloadContents")
                        .HasForeignKey("MessageId");

                    b.HasOne("EdNexusData.Broker.Domain.Request", "Request")
                        .WithMany("PayloadContents")
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.PayloadContentAction", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.Mapping", "ActiveMapping")
                        .WithOne("PrimaryPayloadContentAction")
                        .HasForeignKey("EdNexusData.Broker.Domain.PayloadContentAction", "ActiveMappingId");

                    b.HasOne("EdNexusData.Broker.Domain.PayloadContent", "PayloadContent")
                        .WithMany("PayloadContentActions")
                        .HasForeignKey("PayloadContentId");

                    b.Navigation("ActiveMapping");

                    b.Navigation("PayloadContent");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Request", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.EducationOrganization", "EducationOrganization")
                        .WithMany()
                        .HasForeignKey("EducationOrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EducationOrganization");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.User", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser<System.Guid>", null)
                        .WithOne()
                        .HasForeignKey("EdNexusData.Broker.Domain.User", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.UserRole", b =>
                {
                    b.HasOne("EdNexusData.Broker.Domain.EducationOrganization", "EducationOrganization")
                        .WithMany()
                        .HasForeignKey("EducationOrganizationId");

                    b.HasOne("EdNexusData.Broker.Domain.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId");

                    b.Navigation("EducationOrganization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.EducationOrganization", b =>
                {
                    b.Navigation("EducationOrganizations");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Mapping", b =>
                {
                    b.Navigation("PrimaryPayloadContentAction");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Message", b =>
                {
                    b.Navigation("PayloadContents");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.PayloadContent", b =>
                {
                    b.Navigation("PayloadContentActions");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.PayloadContentAction", b =>
                {
                    b.Navigation("Mappings");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.Request", b =>
                {
                    b.Navigation("Messages");

                    b.Navigation("PayloadContents");
                });

            modelBuilder.Entity("EdNexusData.Broker.Domain.User", b =>
                {
                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
