-- =============================================
-- NGR Database Initial Schema
-- Version: 1.0
-- Database: Azure SQL Database
-- =============================================

-- Create schema for NGR objects
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ngr')
BEGIN
    EXEC('CREATE SCHEMA ngr');
END
GO

-- =============================================
-- LOOKUP TABLES
-- =============================================

-- Roles lookup table
CREATE TABLE ngr.Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Permissions NVARCHAR(MAX), -- JSON array of permissions
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Encounter types lookup
CREATE TABLE ngr.EncounterTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Form status lookup
CREATE TABLE ngr.FormStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);

-- =============================================
-- CARE PROGRAM TABLES
-- =============================================

-- Care Programs (136 CF care centers)
CREATE TABLE ngr.CarePrograms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Address1 NVARCHAR(200),
    Address2 NVARCHAR(200),
    City NVARCHAR(100),
    State NVARCHAR(2),
    ZipCode NVARCHAR(10),
    Phone NVARCHAR(20),
    Email NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    AccreditationDate DATE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
);

CREATE INDEX IX_CarePrograms_Code ON ngr.CarePrograms(Code);
CREATE INDEX IX_CarePrograms_IsActive ON ngr.CarePrograms(IsActive);

-- =============================================
-- USER TABLES
-- =============================================

-- Users (synced from Okta)
CREATE TABLE ngr.Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OktaId NVARCHAR(255) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Users_OktaId ON ngr.Users(OktaId);
CREATE INDEX IX_Users_Email ON ngr.Users(Email);
CREATE INDEX IX_Users_IsActive ON ngr.Users(IsActive);

-- Program-User assignments with roles
CREATE TABLE ngr.ProgramUsers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProgramId INT NOT NULL FOREIGN KEY REFERENCES ngr.CarePrograms(Id),
    UserId INT NOT NULL FOREIGN KEY REFERENCES ngr.Users(Id),
    RoleId INT NOT NULL FOREIGN KEY REFERENCES ngr.Roles(Id),
    IsActive BIT NOT NULL DEFAULT 1,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_ProgramUsers_ProgramUser UNIQUE (ProgramId, UserId)
);

CREATE INDEX IX_ProgramUsers_ProgramId ON ngr.ProgramUsers(ProgramId);
CREATE INDEX IX_ProgramUsers_UserId ON ngr.ProgramUsers(UserId);

-- =============================================
-- PATIENT TABLES
-- =============================================

-- Master Patient Index
CREATE TABLE ngr.Patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegistryId NVARCHAR(20) NOT NULL UNIQUE, -- NGR-assigned unique ID
    MRN NVARCHAR(50), -- Medical Record Number (may vary by program)
    FirstName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100),
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(20),
    SSNLast4 NVARCHAR(4), -- Last 4 digits only, encrypted
    IsDeceased BIT NOT NULL DEFAULT 0,
    DeceasedDate DATE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
);

CREATE INDEX IX_Patients_RegistryId ON ngr.Patients(RegistryId);
CREATE INDEX IX_Patients_LastName_FirstName ON ngr.Patients(LastName, FirstName);
CREATE INDEX IX_Patients_DateOfBirth ON ngr.Patients(DateOfBirth);
CREATE INDEX IX_Patients_IsDeceased ON ngr.Patients(IsDeceased);

-- Patient-Program assignments (roster)
CREATE TABLE ngr.PatientPrograms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL FOREIGN KEY REFERENCES ngr.Patients(Id),
    ProgramId INT NOT NULL FOREIGN KEY REFERENCES ngr.CarePrograms(Id),
    LocalMRN NVARCHAR(50), -- Program-specific MRN
    Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE, TRANSFERRED, REMOVED
    IsPrimaryProgram BIT NOT NULL DEFAULT 0,
    EnrollmentDate DATE NOT NULL,
    DischargeDate DATE,
    DischargeReason NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    CONSTRAINT UQ_PatientPrograms_PatientProgram UNIQUE (PatientId, ProgramId)
);

CREATE INDEX IX_PatientPrograms_PatientId ON ngr.PatientPrograms(PatientId);
CREATE INDEX IX_PatientPrograms_ProgramId ON ngr.PatientPrograms(ProgramId);
CREATE INDEX IX_PatientPrograms_Status ON ngr.PatientPrograms(Status);

-- Patient Demographics (extended info)
CREATE TABLE ngr.PatientDemographics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES ngr.Patients(Id),
    Address1 NVARCHAR(200),
    Address2 NVARCHAR(200),
    City NVARCHAR(100),
    State NVARCHAR(2),
    ZipCode NVARCHAR(10),
    Country NVARCHAR(50) DEFAULT 'USA',
    Phone NVARCHAR(20),
    Email NVARCHAR(255),
    EmergencyContactName NVARCHAR(200),
    EmergencyContactPhone NVARCHAR(20),
    EmergencyContactRelation NVARCHAR(50),
    PrimaryLanguage NVARCHAR(50),
    Ethnicity NVARCHAR(50),
    Race NVARCHAR(100),
    InsuranceType NVARCHAR(50),
    InsuranceProvider NVARCHAR(200),
    InsurancePolicyNumber NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_PatientDemographics_PatientId ON ngr.PatientDemographics(PatientId);

-- Patient Merge History
CREATE TABLE ngr.PatientMergeHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SurvivorPatientId INT NOT NULL FOREIGN KEY REFERENCES ngr.Patients(Id),
    MergedPatientId INT NOT NULL, -- Original ID before merge
    MergedRegistryId NVARCHAR(20) NOT NULL,
    MergeReason NVARCHAR(500),
    MergedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    MergedBy NVARCHAR(255) NOT NULL
);

CREATE INDEX IX_PatientMergeHistory_SurvivorPatientId ON ngr.PatientMergeHistory(SurvivorPatientId);

-- =============================================
-- ENCOUNTER TABLES
-- =============================================

-- Encounters
CREATE TABLE ngr.Encounters (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL FOREIGN KEY REFERENCES ngr.Patients(Id),
    ProgramId INT NOT NULL FOREIGN KEY REFERENCES ngr.CarePrograms(Id),
    EncounterTypeId INT NOT NULL FOREIGN KEY REFERENCES ngr.EncounterTypes(Id),
    EncounterDate DATE NOT NULL,
    EncounterYear INT NOT NULL, -- Computed for reporting
    Status NVARCHAR(20) NOT NULL DEFAULT 'OPEN', -- OPEN, COMPLETE, LOCKED
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
);

CREATE INDEX IX_Encounters_PatientId ON ngr.Encounters(PatientId);
CREATE INDEX IX_Encounters_ProgramId ON ngr.Encounters(ProgramId);
CREATE INDEX IX_Encounters_EncounterDate ON ngr.Encounters(EncounterDate);
CREATE INDEX IX_Encounters_EncounterYear ON ngr.Encounters(EncounterYear);
CREATE INDEX IX_Encounters_Status ON ngr.Encounters(Status);

-- =============================================
-- FORM TABLES
-- =============================================

-- Form Definitions (eCRF templates)
CREATE TABLE ngr.FormDefinitions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Version INT NOT NULL DEFAULT 1,
    SchemaJson NVARCHAR(MAX) NOT NULL, -- JSON Schema for form fields
    ValidationRulesJson NVARCHAR(MAX), -- JSON validation rules
    UISchemaJson NVARCHAR(MAX), -- JSON UI rendering hints
    EncounterTypeId INT FOREIGN KEY REFERENCES ngr.EncounterTypes(Id),
    IsActive BIT NOT NULL DEFAULT 1,
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255),
    CONSTRAINT UQ_FormDefinitions_CodeVersion UNIQUE (Code, Version)
);

CREATE INDEX IX_FormDefinitions_Code ON ngr.FormDefinitions(Code);
CREATE INDEX IX_FormDefinitions_IsActive ON ngr.FormDefinitions(IsActive);

-- Form Submissions (eCRF data)
CREATE TABLE ngr.FormSubmissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FormDefinitionId INT NOT NULL FOREIGN KEY REFERENCES ngr.FormDefinitions(Id),
    PatientId INT NOT NULL FOREIGN KEY REFERENCES ngr.Patients(Id),
    EncounterId INT FOREIGN KEY REFERENCES ngr.Encounters(Id),
    ProgramId INT NOT NULL FOREIGN KEY REFERENCES ngr.CarePrograms(Id),
    StatusId INT NOT NULL FOREIGN KEY REFERENCES ngr.FormStatuses(Id),
    FormDataJson NVARCHAR(MAX) NOT NULL, -- JSON form data
    SubmittedAt DATETIME2,
    SubmittedBy NVARCHAR(255),
    LockedAt DATETIME2,
    LockedBy NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
);

CREATE INDEX IX_FormSubmissions_FormDefinitionId ON ngr.FormSubmissions(FormDefinitionId);
CREATE INDEX IX_FormSubmissions_PatientId ON ngr.FormSubmissions(PatientId);
CREATE INDEX IX_FormSubmissions_EncounterId ON ngr.FormSubmissions(EncounterId);
CREATE INDEX IX_FormSubmissions_ProgramId ON ngr.FormSubmissions(ProgramId);
CREATE INDEX IX_FormSubmissions_StatusId ON ngr.FormSubmissions(StatusId);
CREATE INDEX IX_FormSubmissions_SubmittedAt ON ngr.FormSubmissions(SubmittedAt);

-- Form Field History (audit trail for field changes)
CREATE TABLE ngr.FormFieldHistory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    FormSubmissionId INT NOT NULL FOREIGN KEY REFERENCES ngr.FormSubmissions(Id),
    FieldPath NVARCHAR(255) NOT NULL, -- JSON path to field
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChangedBy NVARCHAR(255) NOT NULL
);

CREATE INDEX IX_FormFieldHistory_FormSubmissionId ON ngr.FormFieldHistory(FormSubmissionId);
CREATE INDEX IX_FormFieldHistory_ChangedAt ON ngr.FormFieldHistory(ChangedAt);

-- =============================================
-- IMPORT TABLES
-- =============================================

-- Import Jobs (CSV uploads)
CREATE TABLE ngr.ImportJobs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProgramId INT NOT NULL FOREIGN KEY REFERENCES ngr.CarePrograms(Id),
    FileName NVARCHAR(255) NOT NULL,
    FileSize BIGINT NOT NULL,
    BlobUrl NVARCHAR(500) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING, PROCESSING, PREVIEW, CONFIRMED, FAILED
    TotalRows INT,
    ProcessedRows INT,
    ErrorRows INT,
    MappingJson NVARCHAR(MAX), -- Field mapping configuration
    ResultsJson NVARCHAR(MAX), -- Processing results
    StartedAt DATETIME2,
    CompletedAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255) NOT NULL
);

CREATE INDEX IX_ImportJobs_ProgramId ON ngr.ImportJobs(ProgramId);
CREATE INDEX IX_ImportJobs_Status ON ngr.ImportJobs(Status);
CREATE INDEX IX_ImportJobs_CreatedAt ON ngr.ImportJobs(CreatedAt);

-- Import Errors
CREATE TABLE ngr.ImportErrors (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ImportJobId INT NOT NULL FOREIGN KEY REFERENCES ngr.ImportJobs(Id),
    RowNumber INT NOT NULL,
    FieldName NVARCHAR(100),
    ErrorType NVARCHAR(50) NOT NULL,
    ErrorMessage NVARCHAR(500) NOT NULL,
    RawData NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_ImportErrors_ImportJobId ON ngr.ImportErrors(ImportJobId);

-- =============================================
-- CONTENT MANAGEMENT TABLES
-- =============================================

-- Content (documents, help articles)
CREATE TABLE ngr.Contents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NOT NULL UNIQUE,
    Category NVARCHAR(50) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    IsPublished BIT NOT NULL DEFAULT 0,
    PublishedAt DATETIME2,
    PublishedBy NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
);

CREATE INDEX IX_Contents_Slug ON ngr.Contents(Slug);
CREATE INDEX IX_Contents_Category ON ngr.Contents(Category);
CREATE INDEX IX_Contents_IsPublished ON ngr.Contents(IsPublished);

-- Announcements
CREATE TABLE ngr.Announcements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT 'NORMAL', -- LOW, NORMAL, HIGH, URGENT
    TargetAudience NVARCHAR(50) NOT NULL DEFAULT 'ALL', -- ALL, FOUNDATION, PROGRAMS
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(255),
    UpdatedBy NVARCHAR(255)
);

CREATE INDEX IX_Announcements_StartDate ON ngr.Announcements(StartDate);
CREATE INDEX IX_Announcements_IsActive ON ngr.Announcements(IsActive);

-- =============================================
-- REPORTING TABLES
-- =============================================

-- Saved Reports (user-created report definitions)
CREATE TABLE ngr.SavedReports (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES ngr.Users(Id),
    ProgramId INT FOREIGN KEY REFERENCES ngr.CarePrograms(Id), -- NULL for Foundation-wide
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    QueryDefinitionJson NVARCHAR(MAX) NOT NULL, -- Report query configuration
    ColumnsJson NVARCHAR(MAX) NOT NULL, -- Selected columns
    FiltersJson NVARCHAR(MAX), -- Applied filters
    SortJson NVARCHAR(MAX), -- Sort configuration
    IsShared BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_SavedReports_UserId ON ngr.SavedReports(UserId);
CREATE INDEX IX_SavedReports_ProgramId ON ngr.SavedReports(ProgramId);

-- Standard Reports (pre-built reports)
CREATE TABLE ngr.StandardReports (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Category NVARCHAR(50) NOT NULL,
    QueryDefinitionJson NVARCHAR(MAX) NOT NULL,
    ColumnsJson NVARCHAR(MAX) NOT NULL,
    AvailableFiltersJson NVARCHAR(MAX),
    RequiredRole NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_StandardReports_Code ON ngr.StandardReports(Code);
CREATE INDEX IX_StandardReports_Category ON ngr.StandardReports(Category);

-- =============================================
-- AUDIT TABLES
-- =============================================

-- Audit Log
CREATE TABLE ngr.AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES ngr.Users(Id),
    UserEmail NVARCHAR(255) NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- CREATE, READ, UPDATE, DELETE, LOGIN, LOGOUT, EXPORT
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(50),
    ProgramId INT FOREIGN KEY REFERENCES ngr.CarePrograms(Id),
    OldValuesJson NVARCHAR(MAX),
    NewValuesJson NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    RequestPath NVARCHAR(500),
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_AuditLogs_UserId ON ngr.AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Action ON ngr.AuditLogs(Action);
CREATE INDEX IX_AuditLogs_EntityType ON ngr.AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_EntityId ON ngr.AuditLogs(EntityId);
CREATE INDEX IX_AuditLogs_Timestamp ON ngr.AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_ProgramId ON ngr.AuditLogs(ProgramId);

-- =============================================
-- DATA WAREHOUSE SYNC TABLES
-- =============================================

-- Data Warehouse Sync Status
CREATE TABLE ngr.DataWarehouseSyncStatus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SyncType NVARCHAR(20) NOT NULL, -- FULL, INCREMENTAL
    EntityType NVARCHAR(100) NOT NULL,
    LastSyncAt DATETIME2 NOT NULL,
    LastSyncId BIGINT, -- Last processed ID for incremental
    RecordsProcessed INT NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- SUCCESS, FAILED, PARTIAL
    ErrorMessage NVARCHAR(MAX),
    Duration INT, -- Seconds
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_DataWarehouseSyncStatus_EntityType ON ngr.DataWarehouseSyncStatus(EntityType);
CREATE INDEX IX_DataWarehouseSyncStatus_LastSyncAt ON ngr.DataWarehouseSyncStatus(LastSyncAt);

-- Change Tracking for incremental sync
CREATE TABLE ngr.ChangeTracking (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    ChangeType NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2,
    IsProcessed BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_ChangeTracking_EntityType_IsProcessed ON ngr.ChangeTracking(EntityType, IsProcessed);
CREATE INDEX IX_ChangeTracking_ChangedAt ON ngr.ChangeTracking(ChangedAt);

GO
