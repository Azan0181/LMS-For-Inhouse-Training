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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [Department] nvarchar(max) NULL,
        [JobTitle] nvarchar(max) NULL,
        [HireDate] datetime2 NULL,
        [ProfileImageUrl] nvarchar(max) NULL,
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [ThumbnailUrl] nvarchar(max) NULL,
        [CreatedById] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsPublished] bit NOT NULL,
        [Category] nvarchar(100) NULL,
        [DifficultyLevel] int NOT NULL,
        [EstimatedDurationMinutes] int NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [CourseAssignments] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [EmployeeId] nvarchar(450) NOT NULL,
        [AssignedById] nvarchar(450) NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        [DueDate] datetime2 NULL,
        [Status] int NOT NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_CourseAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseAssignments_AspNetUsers_AssignedById] FOREIGN KEY ([AssignedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseAssignments_AspNetUsers_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseAssignments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [Videos] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [VideoUrl] nvarchar(max) NOT NULL,
        [OrderIndex] int NOT NULL,
        [DurationSeconds] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Videos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Videos_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [QuizCheckpoints] (
        [Id] int NOT NULL IDENTITY,
        [VideoId] int NOT NULL,
        [TimestampSeconds] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [IsRequired] bit NOT NULL,
        [PassingScorePercent] int NOT NULL,
        CONSTRAINT [PK_QuizCheckpoints] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizCheckpoints_Videos_VideoId] FOREIGN KEY ([VideoId]) REFERENCES [Videos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [VideoProgresses] (
        [Id] int NOT NULL IDENTITY,
        [CourseAssignmentId] int NOT NULL,
        [VideoId] int NOT NULL,
        [LastWatchedPositionSeconds] int NOT NULL,
        [IsCompleted] bit NOT NULL,
        [CompletedAt] datetime2 NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_VideoProgresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VideoProgresses_CourseAssignments_CourseAssignmentId] FOREIGN KEY ([CourseAssignmentId]) REFERENCES [CourseAssignments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_VideoProgresses_Videos_VideoId] FOREIGN KEY ([VideoId]) REFERENCES [Videos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [Questions] (
        [Id] int NOT NULL IDENTITY,
        [CheckpointId] int NOT NULL,
        [QuestionText] nvarchar(1000) NOT NULL,
        [OrderIndex] int NOT NULL,
        [Points] int NOT NULL,
        CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Questions_QuizCheckpoints_CheckpointId] FOREIGN KEY ([CheckpointId]) REFERENCES [QuizCheckpoints] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [QuizAttempts] (
        [Id] int NOT NULL IDENTITY,
        [CourseAssignmentId] int NOT NULL,
        [CheckpointId] int NOT NULL,
        [AttemptedAt] datetime2 NOT NULL,
        [Score] int NOT NULL,
        [MaxScore] int NOT NULL,
        [IsPassed] bit NOT NULL,
        CONSTRAINT [PK_QuizAttempts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizAttempts_CourseAssignments_CourseAssignmentId] FOREIGN KEY ([CourseAssignmentId]) REFERENCES [CourseAssignments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_QuizAttempts_QuizCheckpoints_CheckpointId] FOREIGN KEY ([CheckpointId]) REFERENCES [QuizCheckpoints] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [AnswerOptions] (
        [Id] int NOT NULL IDENTITY,
        [QuestionId] int NOT NULL,
        [OptionText] nvarchar(500) NOT NULL,
        [IsCorrect] bit NOT NULL,
        [OrderIndex] int NOT NULL,
        CONSTRAINT [PK_AnswerOptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnswerOptions_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE TABLE [QuizAnswers] (
        [Id] int NOT NULL IDENTITY,
        [QuizAttemptId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [SelectedAnswerOptionId] int NULL,
        [IsCorrect] bit NOT NULL,
        CONSTRAINT [PK_QuizAnswers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuizAnswers_AnswerOptions_SelectedAnswerOptionId] FOREIGN KEY ([SelectedAnswerOptionId]) REFERENCES [AnswerOptions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuizAnswers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuizAnswers_QuizAttempts_QuizAttemptId] FOREIGN KEY ([QuizAttemptId]) REFERENCES [QuizAttempts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_AnswerOptions_QuestionId] ON [AnswerOptions] ([QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_CourseAssignments_AssignedById] ON [CourseAssignments] ([AssignedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CourseAssignments_CourseId_EmployeeId] ON [CourseAssignments] ([CourseId], [EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_CourseAssignments_EmployeeId] ON [CourseAssignments] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_Courses_CreatedById] ON [Courses] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_Courses_IsPublished] ON [Courses] ([IsPublished]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_Questions_CheckpointId] ON [Questions] ([CheckpointId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_QuestionId] ON [QuizAnswers] ([QuestionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_QuizAttemptId] ON [QuizAnswers] ([QuizAttemptId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_QuizAnswers_SelectedAnswerOptionId] ON [QuizAnswers] ([SelectedAnswerOptionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_QuizAttempts_CheckpointId] ON [QuizAttempts] ([CheckpointId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_QuizAttempts_CourseAssignmentId] ON [QuizAttempts] ([CourseAssignmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_QuizCheckpoints_VideoId_TimestampSeconds] ON [QuizCheckpoints] ([VideoId], [TimestampSeconds]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VideoProgresses_CourseAssignmentId_VideoId] ON [VideoProgresses] ([CourseAssignmentId], [VideoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_VideoProgresses_VideoId] ON [VideoProgresses] ([VideoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    CREATE INDEX [IX_Videos_CourseId_OrderIndex] ON [Videos] ([CourseId], [OrderIndex]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261012063104_InitialSqlServerCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261012063104_InitialSqlServerCreate', N'9.0.0');
END;

COMMIT;
GO

