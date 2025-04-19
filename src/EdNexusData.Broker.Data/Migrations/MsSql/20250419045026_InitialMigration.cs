using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdNexusData.Broker.Data.Migrations.MsSql
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__BrokerSeedsHistory",
                columns: table => new
                {
                    SeedId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SeedName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___BrokerSeedsHistory", x => x.SeedId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectorReference",
                columns: table => new
                {
                    ConnectorReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectorReference", x => x.ConnectorReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EducationOrganizations",
                columns: table => new
                {
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationOrganizationType = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TimeZone = table.Column<string>(type: "varchar(50)", nullable: true),
                    Contacts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizations", x => x.EducationOrganizationId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizations_EducationOrganizations_ParentOrganizationId",
                        column: x => x.ParentOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false),
                    AllEducationOrganizations = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EducationOrganizationConnectorSettings",
                columns: table => new
                {
                    EducationOrganizationConnectorSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Connector = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizationConnectorSettings", x => x.EducationOrganizationConnectorSettingsId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizationConnectorSettings_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateTable(
                name: "EducationOrganizationPayloadSettings",
                columns: table => new
                {
                    EducationOrganizationPayloadSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncomingPayloadSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingPayloadSettings = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationOrganizationPayloadSettings", x => x.EducationOrganizationPayloadSettingsId);
                    table.ForeignKey(
                        name: "FK_EducationOrganizationPayloadSettings_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Student = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestProcessUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitialRequestSentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequestManifest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseManifest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseProcessUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncomingOutgoing = table.Column<int>(type: "int", nullable: false),
                    RequestStatus = table.Column<int>(type: "int", nullable: false),
                    MatchDisposition = table.Column<int>(type: "int", nullable: true),
                    Open = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_Users_RequestProcessUserId",
                        column: x => x.RequestProcessUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Requests_Users_ResponseProcessUserId",
                        column: x => x.ResponseProcessUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRoles_EducationOrganizations_EducationOrganizationId",
                        column: x => x.EducationOrganizationId,
                        principalTable: "EducationOrganizations",
                        principalColumn: "EducationOrganizationId");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Worker_Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StartDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FinishDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobStatus = table.Column<int>(type: "int", nullable: false),
                    WorkerInstance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkerState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkerLog = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InitiatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worker_Jobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_Worker_Jobs_Users_InitiatedUserId",
                        column: x => x.InitiatedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestResponse = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SentTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MessageContents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransmissionDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestStatus = table.Column<int>(type: "int", nullable: true),
                    MessageStatus = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayloadContents",
                columns: table => new
                {
                    PayloadContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlobContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    JsonContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlContent = table.Column<string>(type: "xml", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayloadContents", x => x.PayloadContentId);
                    table.ForeignKey(
                        name: "FK_PayloadContents_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "MessageId");
                    table.ForeignKey(
                        name: "FK_PayloadContents_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadContentActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OriginalSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MappingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentAttributes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonSourceMapping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonInitialMapping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonDestinationMapping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mappings", x => x.MappingId);
                });

            migrationBuilder.CreateTable(
                name: "PayloadContentActions",
                columns: table => new
                {
                    PayloadContentActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayloadContentActionType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ActiveMappingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Process = table.Column<bool>(type: "bit", nullable: false),
                    PayloadContentActionStatus = table.Column<int>(type: "int", nullable: false),
                    ProcessState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayloadContentActions", x => x.PayloadContentActionId);
                    table.ForeignKey(
                        name: "FK_PayloadContentActions_Mappings_ActiveMappingId",
                        column: x => x.ActiveMappingId,
                        principalTable: "Mappings",
                        principalColumn: "MappingId");
                    table.ForeignKey(
                        name: "FK_PayloadContentActions_PayloadContents_PayloadContentId",
                        column: x => x.PayloadContentId,
                        principalTable: "PayloadContents",
                        principalColumn: "PayloadContentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectorReference_Reference",
                table: "ConnectorReference",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationConnectorSettings_EducationOrganizationId_Connector",
                table: "EducationOrganizationConnectorSettings",
                columns: new[] { "EducationOrganizationId", "Connector" },
                unique: true,
                filter: "[EducationOrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload",
                table: "EducationOrganizationPayloadSettings",
                columns: new[] { "EducationOrganizationId", "Payload" },
                unique: true,
                filter: "[EducationOrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizations_Domain",
                table: "EducationOrganizations",
                column: "Domain",
                unique: true,
                filter: "[Domain] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EducationOrganizations_ParentOrganizationId",
                table: "EducationOrganizations",
                column: "ParentOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Mappings_PayloadContentActionId_Version",
                table: "Mappings",
                columns: new[] { "PayloadContentActionId", "Version" },
                unique: true,
                filter: "[PayloadContentActionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RequestId",
                table: "Messages",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_ActiveMappingId",
                table: "PayloadContentActions",
                column: "ActiveMappingId",
                unique: true,
                filter: "[ActiveMappingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContentActions_PayloadContentId_PayloadContentActionType",
                table: "PayloadContentActions",
                columns: new[] { "PayloadContentId", "PayloadContentActionType" },
                unique: true,
                filter: "[PayloadContentId] IS NOT NULL AND [PayloadContentActionType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContents_MessageId",
                table: "PayloadContents",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PayloadContents_RequestId",
                table: "PayloadContents",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_EducationOrganizationId",
                table: "Requests",
                column: "EducationOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestProcessUserId",
                table: "Requests",
                column: "RequestProcessUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ResponseProcessUserId",
                table: "Requests",
                column: "ResponseProcessUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EducationOrganizationId_UserId",
                table: "UserRoles",
                columns: new[] { "EducationOrganizationId", "UserId" },
                unique: true,
                filter: "[EducationOrganizationId] IS NOT NULL AND [UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Worker_Jobs_InitiatedUserId",
                table: "Worker_Jobs",
                column: "InitiatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mappings_PayloadContentActions_PayloadContentActionId",
                table: "Mappings",
                column: "PayloadContentActionId",
                principalTable: "PayloadContentActions",
                principalColumn: "PayloadContentActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspNetUsers_UserId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_EducationOrganizations_EducationOrganizationId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Mappings_PayloadContentActions_PayloadContentActionId",
                table: "Mappings");

            migrationBuilder.DropTable(
                name: "__BrokerSeedsHistory");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ConnectorReference");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "EducationOrganizationConnectorSettings");

            migrationBuilder.DropTable(
                name: "EducationOrganizationPayloadSettings");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Worker_Jobs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "EducationOrganizations");

            migrationBuilder.DropTable(
                name: "PayloadContentActions");

            migrationBuilder.DropTable(
                name: "Mappings");

            migrationBuilder.DropTable(
                name: "PayloadContents");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
