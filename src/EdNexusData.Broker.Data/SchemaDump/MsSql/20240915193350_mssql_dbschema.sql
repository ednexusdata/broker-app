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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE TABLE [EducationOrganizations] (
        [EducationOrganizationId] uniqueidentifier NOT NULL,
        [ParentOrganizationId] uniqueidentifier NULL,
        [Name] nvarchar(max) NOT NULL,
        [Number] nvarchar(max) NULL,
        [EducationOrganizationType] int NOT NULL,
        [StreetNumberName] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [StateAbbreviation] nvarchar(max) NULL,
        [PostalCode] nvarchar(max) NULL,
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [IsSuperAdmin] bit NOT NULL,
        [AllEducationOrganizations] int NOT NULL,
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE TABLE [EducationOrganizationPayloadSettings] (
        [EducationOrganizationPayloadSettingsId] uniqueidentifier NOT NULL,
        [EducationOrganizationId] uniqueidentifier NULL,
        [PayloadDirection] int NULL,
        [Payload] nvarchar(450) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [Settings] nvarchar(max) NULL,
        CONSTRAINT [PK_EducationOrganizationPayloadSettings] PRIMARY KEY ([EducationOrganizationPayloadSettingsId]),
        CONSTRAINT [FK_EducationOrganizationPayloadSettings_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE TABLE [Requests] (
        [RequestId] uniqueidentifier NOT NULL,
        [EducationOrganizationId] uniqueidentifier NULL,
        [Student] nvarchar(max) NULL,
        [RequestManifest] nvarchar(max) NULL,
        [RequestProcessUserId] uniqueidentifier NULL,
        [InitialRequestSentDate] datetime2 NULL,
        [ResponseManifest] nvarchar(max) NULL,
        [ResponseProcessUserId] uniqueidentifier NULL,
        [RequestStatus] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Requests] PRIMARY KEY ([RequestId]),
        CONSTRAINT [FK_Requests_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE TABLE [Messages] (
        [MessageId] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [RequestResponse] int NOT NULL,
        [MessageTimestamp] datetime2 NOT NULL,
        [MessageContents] nvarchar(max) NULL,
        [TransmissionDetails] nvarchar(max) NULL,
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE TABLE [PayloadContents] (
        [PayloadContentId] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [MessageId] uniqueidentifier NULL,
        [ContentType] nvarchar(max) NULL,
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
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizationConnectorSettings_EducationOrganizationId_Connector] ON [EducationOrganizationConnectorSettings] ([EducationOrganizationId], [Connector]) WHERE [EducationOrganizationId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload_PayloadDirection] ON [EducationOrganizationPayloadSettings] ([EducationOrganizationId], [Payload], [PayloadDirection]) WHERE [EducationOrganizationId] IS NOT NULL AND [PayloadDirection] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_EducationOrganizations_ParentOrganizationId] ON [EducationOrganizations] ([ParentOrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_Messages_RequestId] ON [Messages] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_PayloadContents_MessageId] ON [PayloadContents] ([MessageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_PayloadContents_RequestId] ON [PayloadContents] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_Requests_EducationOrganizationId] ON [Requests] ([EducationOrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserRoles_EducationOrganizationId_UserId] ON [UserRoles] ([EducationOrganizationId], [UserId]) WHERE [EducationOrganizationId] IS NOT NULL AND [UserId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    CREATE INDEX [IX_UserRoles_UserId] ON [UserRoles] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231220045331_InitialRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231220045331_InitialRequests', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    DROP INDEX [IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload_PayloadDirection] ON [EducationOrganizationPayloadSettings];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EducationOrganizationPayloadSettings]') AND [c].[name] = N'PayloadDirection');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [EducationOrganizationPayloadSettings] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [EducationOrganizationPayloadSettings] DROP COLUMN [PayloadDirection];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    EXEC sp_rename N'[EducationOrganizationPayloadSettings].[Settings]', N'OutgoingPayloadSettings', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    ALTER TABLE [Requests] ADD [ProcessState] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    ALTER TABLE [Requests] ADD [WorkerInstance] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    ALTER TABLE [PayloadContents] ADD [FileName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Messages]') AND [c].[name] = N'MessageTimestamp');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Messages] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Messages] ALTER COLUMN [MessageTimestamp] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    ALTER TABLE [EducationOrganizationPayloadSettings] ADD [IncomingPayloadSettings] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizationPayloadSettings_EducationOrganizationId_Payload] ON [EducationOrganizationPayloadSettings] ([EducationOrganizationId], [Payload]) WHERE [EducationOrganizationId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240119175548_AddWorkerAndMessages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240119175548_AddWorkerAndMessages', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    ALTER TABLE [Requests] DROP CONSTRAINT [FK_Requests_EducationOrganizations_EducationOrganizationId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EducationOrganizations]') AND [c].[name] = N'City');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [EducationOrganizations] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [EducationOrganizations] DROP COLUMN [City];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EducationOrganizations]') AND [c].[name] = N'PostalCode');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [EducationOrganizations] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [EducationOrganizations] DROP COLUMN [PostalCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    EXEC sp_rename N'[EducationOrganizations].[StreetNumberName]', N'Contacts', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    EXEC sp_rename N'[EducationOrganizations].[StateAbbreviation]', N'Address', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    DROP INDEX [IX_Requests_EducationOrganizationId] ON [Requests];
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Requests]') AND [c].[name] = N'EducationOrganizationId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Requests] DROP CONSTRAINT [' + @var4 + '];');
    EXEC(N'UPDATE [Requests] SET [EducationOrganizationId] = ''00000000-0000-0000-0000-000000000000'' WHERE [EducationOrganizationId] IS NULL');
    ALTER TABLE [Requests] ALTER COLUMN [EducationOrganizationId] uniqueidentifier NOT NULL;
    ALTER TABLE [Requests] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [EducationOrganizationId];
    CREATE INDEX [IX_Requests_EducationOrganizationId] ON [Requests] ([EducationOrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    ALTER TABLE [Requests] ADD [IncomingOutgoing] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    ALTER TABLE [Requests] ADD [Payload] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    ALTER TABLE [EducationOrganizations] ADD [Domain] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EducationOrganizations_Domain] ON [EducationOrganizations] ([Domain]) WHERE [Domain] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    ALTER TABLE [Requests] ADD CONSTRAINT [FK_Requests_EducationOrganizations_EducationOrganizationId] FOREIGN KEY ([EducationOrganizationId]) REFERENCES [EducationOrganizations] ([EducationOrganizationId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240216040258_LatesttoMapper'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240216040258_LatesttoMapper', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240220053830_InitialMapping'
)
BEGIN
    CREATE TABLE [Mappings] (
        [MappingId] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NULL,
        [OriginalSchema] nvarchar(max) NULL,
        [MappingType] nvarchar(max) NULL,
        [StudentAttributes] nvarchar(max) NULL,
        [SourceMapping] nvarchar(max) NULL,
        [DestinationMapping] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Mappings] PRIMARY KEY ([MappingId]),
        CONSTRAINT [FK_Mappings_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [Requests] ([RequestId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240220053830_InitialMapping'
)
BEGIN
    CREATE INDEX [IX_Mappings_RequestId] ON [Mappings] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240220053830_InitialMapping'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240220053830_InitialMapping', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240303072259_FixDates'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Requests]') AND [c].[name] = N'InitialRequestSentDate');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Requests] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Requests] ALTER COLUMN [InitialRequestSentDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240303072259_FixDates'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Messages]') AND [c].[name] = N'MessageTimestamp');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Messages] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Messages] ALTER COLUMN [MessageTimestamp] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240303072259_FixDates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240303072259_FixDates', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    ALTER TABLE [Mappings] DROP CONSTRAINT [FK_Mappings_Requests_RequestId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    DROP INDEX [IX_Mappings_RequestId] ON [Mappings];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    EXEC sp_rename N'[Mappings].[SourceMapping]', N'JsonSourceMapping', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    EXEC sp_rename N'[Mappings].[RequestId]', N'PayloadContentActionId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    EXEC sp_rename N'[Mappings].[DestinationMapping]', N'JsonInitialMapping', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    ALTER TABLE [Mappings] ADD [JsonDestinationMapping] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    ALTER TABLE [Mappings] ADD [Version] tinyint NOT NULL DEFAULT CAST(1 AS tinyint);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
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
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
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
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
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
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Worker_Jobs] PRIMARY KEY ([JobId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Mappings_PayloadContentActionId_Version] ON [Mappings] ([PayloadContentActionId], [Version]) WHERE [PayloadContentActionId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PayloadContentActions_ActiveMappingId] ON [PayloadContentActions] ([ActiveMappingId]) WHERE [ActiveMappingId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PayloadContentActions_PayloadContentId_PayloadContentActionType] ON [PayloadContentActions] ([PayloadContentId], [PayloadContentActionType]) WHERE [PayloadContentId] IS NOT NULL AND [PayloadContentActionType] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    ALTER TABLE [Mappings] ADD CONSTRAINT [FK_Mappings_PayloadContentActions_PayloadContentActionId] FOREIGN KEY ([PayloadContentActionId]) REFERENCES [PayloadContentActions] ([PayloadContentActionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240825183755_LatestChanges202408'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240825183755_LatestChanges202408', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240915193333_AspNetCoreDataProtection'
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
    WHERE [MigrationId] = N'20240915193333_AspNetCoreDataProtection'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240915193333_AspNetCoreDataProtection', N'8.0.8');
END;
GO

COMMIT;
GO

