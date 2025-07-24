-- Azure SQL Database Table Definitions for Requirements & Test Management Tool

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    Role NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Requirements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Type NVARCHAR(20) NOT NULL, -- CRS, PRS, SRS
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ParentId INT NULL,
    Status NVARCHAR(50) NOT NULL,
    Version INT NOT NULL DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Requirements_Parent FOREIGN KEY (ParentId) REFERENCES Requirements(Id),
    CONSTRAINT FK_Requirements_User FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE TABLE TestSuites (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TestSuites_User FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE TABLE TestCases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SuiteId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Steps NVARCHAR(MAX),
    ExpectedResult NVARCHAR(MAX),
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TestCases_Suite FOREIGN KEY (SuiteId) REFERENCES TestSuites(Id),
    CONSTRAINT FK_TestCases_User FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE TABLE RequirementTestCaseLinks (
    RequirementId INT NOT NULL,
    TestCaseId INT NOT NULL,
    PRIMARY KEY (RequirementId, TestCaseId),
    CONSTRAINT FK_RTC_Requirement FOREIGN KEY (RequirementId) REFERENCES Requirements(Id),
    CONSTRAINT FK_RTC_TestCase FOREIGN KEY (TestCaseId) REFERENCES TestCases(Id)
);

CREATE TABLE TestRuns (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TestCaseId INT NOT NULL,
    RunBy INT NOT NULL,
    RunAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Result NVARCHAR(20) NOT NULL, -- Passed, Failed, Blocked, Not Run
    Notes NVARCHAR(MAX),
    EvidenceUrl NVARCHAR(500),
    CONSTRAINT FK_TestRuns_TestCase FOREIGN KEY (TestCaseId) REFERENCES TestCases(Id),
    CONSTRAINT FK_TestRuns_User FOREIGN KEY (RunBy) REFERENCES Users(Id)
);

CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(200) NOT NULL,
    Entity NVARCHAR(50),
    EntityId INT,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Details NVARCHAR(MAX),
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
