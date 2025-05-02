IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [__BrokerSeedsHistory] (
        [SeedId] nvarchar(450) NOT NULL,
        [SeedName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK___BrokerSeedsHistory] PRIMARY KEY ([SeedId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [ConnectorReference] (
        [ConnectorReferenceId] uniqueidentifier NOT NULL,
        [Reference] nvarchar(450) NOT NULL,
        [Version] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ConnectorReference] PRIMARY KEY ([ConnectorReferenceId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [DataProtectionKeys] (
        [Id] int NOT NULL IDENTITY,
        [FriendlyName] nvarchar(max) NULL,
        [Xml] nvarchar(max) NULL,
        CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [EducationOrganizations] (
        [EducationOrganizationId] uniqueidentifier NOT NULL,
        [ParentOrganizationId] uniqueidentifier NULL,
        [Name] nvarchar(max) NOT NULL,
        [ShortName] nvarchar(max) NOT NULL,
        [Number] nvarchar(max) NULL,
        [EducationOrganizationType] int NOT NULL,
        [Address] nvarchar(max) NULL,
        [Domain] nvarchar(450) NULL,
        [TimeZone] varchar(50) NULL,
        [Contacts] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EducationOrganizations] PRIMARY KEY ([EducationOrganizationId]),
        CONSTRAINT [FK_EducationOrganizations_EducationOrganizations_ParentOrganizationId] FOREIGN KEY ([ParentOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [IsSuperAdmin] bit NOT NULL,
        [AllEducationOrganizations] int NOT NULL,
        [TimeZone] varchar(50) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_Users_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [EducationOrganizationConnectorSettings] (
        [EducationOrganizationConnectorSettingsId] uniqueidentifier NOT NULL,
        [EducationOrganizationId] uniqueidentifier NULL,
        [Connector] nvarchar(450) NOT NULL,
        [Settings] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EducationOrganizationConnectorSettings] PRIMARY KEY ([EducationOrganizationConnectorSettingsId]),
        CONSTRAINT [FK_EducationOrganizationConnectorSettings_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [EducationOrganizationPayloadSettings] (
        [EducationOrganizationPayloadSettingsId] uniqueidentifier NOT NULL,
        [EducationOrganizationId] uniqueidentifier NULL,
        [Payload] nvarchar(450) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IncomingPayloadSettings] nvarchar(max) NULL,
        [OutgoingPayloadSettings] nvarchar(max) NULL,
        CONSTRAINT [PK_EducationOrganizationPayloadSettings] PRIMARY KEY ([EducationOrganizationPayloadSettingsId]),
        CONSTRAINT [FK_EducationOrganizationPayloadSettings_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [Requests] (
        [RequestId] uniqueidentifier NOT NULL,
        [EducationOrganizationId] uniqueidentifier NOT NULL,
        [Student] nvarchar(max) NULL,
        [Payload] nvarchar(max) NOT NULL,
        [RequestProcessUserId] uniqueidentifier NULL,
        [InitialRequestSentDate] datetimeoffset NULL,
        [RequestManifest] nvarchar(max) NULL,
        [ResponseManifest] nvarchar(max) NULL,
        [ResponseProcessUserId] uniqueidentifier NULL,
        [IncomingOutgoing] int NOT NULL,
        [RequestStatus] int NOT NULL,
        [MatchDisposition] int NULL,
        [Open] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Requests] PRIMARY KEY ([RequestId]),
        CONSTRAINT [FK_Requests_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Requests_Users_RequestProcessUserId] FOREIGN KEY ([RequestProcessUserId]) REFERENCES [Users] ([UserId]),
        CONSTRAINT [FK_Requests_Users_ResponseProcessUserId] FOREIGN KEY ([ResponseProcessUserId]) REFERENCES [Users] ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserRoleId] uniqueidentifier NOT NULL,
        [EducationOrganizationId] uniqueidentifier NULL,
        [UserId] uniqueidentifier NULL,
        [Role] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId]),
        CONSTRAINT [FK_UserRoles_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId]),
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [Worker_Jobs] (
        [JobId] uniqueidentifier NOT NULL,
        [QueueDateTime] datetimeoffset NOT NULL,
        [StartDateTime] datetimeoffset NULL,
        [FinishDateTime] datetimeoffset NULL,
        [JobType] nvarchar(max) NULL,
        [JobParameters] nvarchar(max) NULL,
        [ReferenceType] nvarchar(max) NULL,
        [ReferenceGuid] uniqueidentifier NULL,
        [JobStatus] int NOT NULL,
        [WorkerInstance] nvarchar(max) NULL,
        [WorkerState] nvarchar(max) NULL,
        [WorkerLog] nvarchar(max) NULL,
        [InitiatedUserId] uniqueidentifier NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Worker_Jobs] PRIMARY KEY ([JobId]),
        CONSTRAINT [FK_Worker_Jobs_Users_InitiatedUserId] FOREIGN KEY ([InitiatedUserId]) REFERENCES [Users] ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [Messages] (
        [MessageId] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [RequestResponse] int NOT NULL,
        [MessageType] nvarchar(max) NULL,
        [MessageTimestamp] datetimeoffset NULL,
        [SentTimestamp] datetimeoffset NULL,
        [MessageContents] nvarchar(max) NULL,
        [TransmissionDetails] nvarchar(max) NULL,
        [RequestStatus] int NULL,
        [MessageStatus] int NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([MessageId]),
        CONSTRAINT [FK_Messages_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [Requests] ([RequestId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [PayloadContents] (
        [PayloadContentId] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [MessageId] uniqueidentifier NULL,
        [ContentType] nvarchar(max) NULL,
        [FileName] nvarchar(max) NULL,
        [BlobContent] varbinary(max) NULL,
        [JsonContent] nvarchar(max) NULL,
        [XmlContent] xml NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PayloadContents] PRIMARY KEY ([PayloadContentId]),
        CONSTRAINT [FK_PayloadContents_Messages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Messages] ([MessageId]),
        CONSTRAINT [FK_PayloadContents_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [Requests] ([RequestId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [Mappings] (
        [MappingId] uniqueidentifier NOT NULL,
        [PayloadContentActionId] uniqueidentifier NULL,
        [OriginalSchema] nvarchar(max) NULL,
        [MappingType] nvarchar(max) NULL,
        [StudentAttributes] nvarchar(max) NULL,
        [JsonSourceMapping] nvarchar(max) NULL,
        [JsonInitialMapping] nvarchar(max) NULL,
        [JsonDestinationMapping] nvarchar(max) NULL,
        [Version] tinyint NOT NULL DEFAULT CAST(1 AS tinyint),
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Mappings] PRIMARY KEY ([MappingId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE TABLE [PayloadContentActions] (
        [PayloadContentActionId] uniqueidentifier NOT NULL,
        [PayloadContentId] uniqueidentifier NULL,
        [PayloadContentActionType] nvarchar(450) NULL,
        [ActiveMappingId] uniqueidentifier NULL,
        [Settings] nvarchar(max) NULL,
        [Process] bit NOT NULL,
        [PayloadContentActionStatus] int NOT NULL,
        [ProcessState] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PayloadContentActions] PRIMARY KEY ([PayloadContentActionId]),
        CONSTRAINT [FK_PayloadContentActions_Mappings_ActiveMappingId] FOREIGN KEY ([ActiveMappingId]) REFERENCES [Mappings] ([MappingId]),
        CONSTRAINT [FK_PayloadContentActions_PayloadContents_PayloadContentId] FOREIGN KEY ([PayloadContentId]) REFERENCES [PayloadContents] ([PayloadContentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConnectorReference_Reference] ON [ConnectorReference] ([Reference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizationConnectorSettings_EducationOrganizationId_Connector] ON [EducationOrganizationConnectorSettings] ([EducationOrganizationId], [Connector]) WHERE [EducationOrganizationId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload] ON [EducationOrganizationPayloadSettings] ([EducationOrganizationId], [Payload]) WHERE [EducationOrganizationId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizations_Domain] ON [EducationOrganizations] ([Domain]) WHERE [Domain] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_EducationOrganizations_ParentOrganizationId] ON [EducationOrganizations] ([ParentOrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Mappings_PayloadContentActionId_Version] ON [Mappings] ([PayloadContentActionId], [Version]) WHERE [PayloadContentActionId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Messages_RequestId] ON [Messages] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PayloadContentActions_ActiveMappingId] ON [PayloadContentActions] ([ActiveMappingId]) WHERE [ActiveMappingId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PayloadContentActions_PayloadContentId_PayloadContentActionType] ON [PayloadContentActions] ([PayloadContentId], [PayloadContentActionType]) WHERE [PayloadContentId] IS NOT NULL AND [PayloadContentActionType] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PayloadContents_MessageId] ON [PayloadContents] ([MessageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PayloadContents_RequestId] ON [PayloadContents] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Requests_EducationOrganizationId] ON [Requests] ([EducationOrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Requests_RequestProcessUserId] ON [Requests] ([RequestProcessUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Requests_ResponseProcessUserId] ON [Requests] ([ResponseProcessUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserRoles_EducationOrganizationId_UserId] ON [UserRoles] ([EducationOrganizationId], [UserId]) WHERE [EducationOrganizationId] IS NOT NULL AND [UserId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_UserRoles_UserId] ON [UserRoles] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Worker_Jobs_InitiatedUserId] ON [Worker_Jobs] ([InitiatedUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    ALTER TABLE [Mappings] ADD CONSTRAINT [FK_Mappings_PayloadContentActions_PayloadContentActionId] FOREIGN KEY ([PayloadContentActionId]) REFERENCES [PayloadContentActions] ([PayloadContentActionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250419045026_InitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250419045026_InitialMigration', N'8.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250502134638_EnabledForEdOrgConnector'
)
BEGIN
    ALTER TABLE [EducationOrganizationConnectorSettings] ADD [Enabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250502134638_EnabledForEdOrgConnector'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250502134638_EnabledForEdOrgConnector', N'8.0.15');
END;
GO

COMMIT;
GO

