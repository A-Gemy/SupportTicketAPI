USE master;
GO

IF DB_ID('SupportTicketDB') IS NULL
BEGIN
    CREATE DATABASE SupportTicketDB;
END
GO

USE SupportTicketDB;
GO

-- =====================================================
-- Users
-- =====================================================

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Customer', 'Agent', 'Admin'))
);
GO

-- =====================================================
-- RefreshTokens
-- =====================================================

CREATE TABLE RefreshTokens
(
    RefreshTokenId INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    TokenHash NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    RevokedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_RefreshTokens PRIMARY KEY (RefreshTokenId),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- =====================================================
-- Tickets
-- =====================================================

CREATE TABLE Tickets
(
    TicketId INT IDENTITY(1,1) NOT NULL,
    CustomerId INT NOT NULL,
    AssignedAgentId INT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Tickets_Status DEFAULT 'Open',
    Priority NVARCHAR(20) NOT NULL CONSTRAINT DF_Tickets_Priority DEFAULT 'Medium',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ClosedAt DATETIME2 NULL,

    CONSTRAINT PK_Tickets PRIMARY KEY (TicketId),

    CONSTRAINT FK_Tickets_Customers 
        FOREIGN KEY (CustomerId) REFERENCES Users(UserId),

    CONSTRAINT FK_Tickets_AssignedAgents 
        FOREIGN KEY (AssignedAgentId) REFERENCES Users(UserId),

    CONSTRAINT CK_Tickets_Status 
        CHECK (Status IN ('Open', 'Assigned', 'InProgress', 'Resolved', 'Closed')),

    CONSTRAINT CK_Tickets_Priority 
        CHECK (Priority IN ('Low', 'Medium', 'High'))
);
GO

-- =====================================================
-- TicketComments
-- =====================================================

CREATE TABLE TicketComments
(
    CommentId INT IDENTITY(1,1) NOT NULL,
    TicketId INT NOT NULL,
    UserId INT NOT NULL,
    CommentText NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TicketComments_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_TicketComments PRIMARY KEY (CommentId),

    CONSTRAINT FK_TicketComments_Tickets 
        FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId),

    CONSTRAINT FK_TicketComments_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- =====================================================
-- AuditLogs
-- =====================================================

CREATE TABLE AuditLogs
(
    AuditLogId INT IDENTITY(1,1) NOT NULL,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    EntityName NVARCHAR(100) NULL,
    EntityId INT NULL,
    Details NVARCHAR(1000) NULL,
    IpAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId),

    CONSTRAINT FK_AuditLogs_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- =====================================================
-- Indexes
-- =====================================================

CREATE INDEX IX_RefreshTokens_UserId
ON RefreshTokens(UserId);
GO

CREATE INDEX IX_RefreshTokens_TokenHash
ON RefreshTokens(TokenHash);
GO

CREATE INDEX IX_Tickets_CustomerId
ON Tickets(CustomerId);
GO

CREATE INDEX IX_Tickets_AssignedAgentId
ON Tickets(AssignedAgentId);
GO

CREATE INDEX IX_Tickets_Status
ON Tickets(Status);
GO

CREATE INDEX IX_TicketComments_TicketId
ON TicketComments(TicketId);
GO

CREATE INDEX IX_AuditLogs_UserId
ON AuditLogs(UserId);
GO

CREATE INDEX IX_AuditLogs_CreatedAt
ON AuditLogs(CreatedAt);
GO