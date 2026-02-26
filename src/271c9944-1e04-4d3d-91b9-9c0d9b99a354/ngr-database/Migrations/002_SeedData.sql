-- =============================================
-- NGR Database Seed Data
-- Version: 1.0
-- =============================================

-- =============================================
-- SEED ROLES
-- =============================================

SET IDENTITY_INSERT ngr.Roles ON;

INSERT INTO ngr.Roles (Id, Name, Description, Permissions) VALUES
(1, 'SystemAdmin', 'Full system access', '["*"]'),
(2, 'FoundationAnalyst', 'Foundation staff with read access across programs', '["patients:read", "reports:*", "content:*"]'),
(3, 'ProgramAdmin', 'Care program administrator', '["patients:*", "users:manage", "reports:*"]'),
(4, 'ClinicalUser', 'Clinical staff with patient access', '["patients:read", "patients:write", "forms:*", "reports:read"]'),
(5, 'ReadOnlyUser', 'Read-only access to program data', '["patients:read", "reports:read"]');

SET IDENTITY_INSERT ngr.Roles OFF;

-- =============================================
-- SEED ENCOUNTER TYPES
-- =============================================

SET IDENTITY_INSERT ngr.EncounterTypes ON;

INSERT INTO ngr.EncounterTypes (Id, Code, Name, Description) VALUES
(1, 'ANNUAL', 'Annual Review', 'Annual patient review encounter'),
(2, 'QUARTERLY', 'Quarterly Visit', 'Quarterly follow-up visit'),
(3, 'HOSPITALIZATION', 'Hospitalization', 'Inpatient hospitalization'),
(4, 'PULMONARY_EXACERBATION', 'Pulmonary Exacerbation', 'Pulmonary exacerbation event'),
(5, 'TRANSPLANT', 'Transplant', 'Transplant-related encounter'),
(6, 'DEATH', 'Death', 'Patient death record');

SET IDENTITY_INSERT ngr.EncounterTypes OFF;

-- =============================================
-- SEED FORM STATUSES
-- =============================================

SET IDENTITY_INSERT ngr.FormStatuses ON;

INSERT INTO ngr.FormStatuses (Id, Code, Name, Description) VALUES
(1, 'DRAFT', 'Draft', 'Form is in draft state'),
(2, 'SUBMITTED', 'Submitted', 'Form has been submitted'),
(3, 'LOCKED', 'Locked', 'Form is locked for editing'),
(4, 'ARCHIVED', 'Archived', 'Form has been archived');

SET IDENTITY_INSERT ngr.FormStatuses OFF;

-- =============================================
-- SEED SAMPLE CARE PROGRAMS
-- =============================================

-- Note: In production, all 136 CF care centers would be loaded
-- This is a sample of representative programs

SET IDENTITY_INSERT ngr.CarePrograms ON;

INSERT INTO ngr.CarePrograms (Id, Code, Name, Address1, City, State, ZipCode, Phone, Email, IsActive, CreatedBy) VALUES
(1, 'CF-001', 'Johns Hopkins Hospital CF Center', '600 N Wolfe St', 'Baltimore', 'MD', '21287', '410-955-5000', 'cf@jhmi.edu', 1, 'SYSTEM'),
(2, 'CF-002', 'Boston Children''s Hospital CF Center', '300 Longwood Ave', 'Boston', 'MA', '02115', '617-355-6000', 'cf@childrens.harvard.edu', 1, 'SYSTEM'),
(3, 'CF-003', 'Stanford Children''s Health CF Center', '725 Welch Rd', 'Palo Alto', 'CA', '94304', '650-723-4000', 'cf@stanfordchildrens.org', 1, 'SYSTEM'),
(4, 'CF-004', 'Children''s Hospital of Philadelphia CF Center', '3401 Civic Center Blvd', 'Philadelphia', 'PA', '19104', '215-590-1000', 'cf@chop.edu', 1, 'SYSTEM'),
(5, 'CF-005', 'Seattle Children''s Hospital CF Center', '4800 Sand Point Way NE', 'Seattle', 'WA', '98105', '206-987-2000', 'cf@seattlechildrens.org', 1, 'SYSTEM'),
(6, 'CF-006', 'Children''s Hospital Colorado CF Center', '13123 E 16th Ave', 'Aurora', 'CO', '80045', '720-777-1234', 'cf@childrenscolorado.org', 1, 'SYSTEM'),
(7, 'CF-007', 'Ann & Robert H. Lurie Children''s Hospital CF Center', '225 E Chicago Ave', 'Chicago', 'IL', '60611', '312-227-4000', 'cf@luriechildrens.org', 1, 'SYSTEM'),
(8, 'CF-008', 'Texas Children''s Hospital CF Center', '6621 Fannin St', 'Houston', 'TX', '77030', '832-824-1000', 'cf@texaschildrens.org', 1, 'SYSTEM'),
(9, 'CF-009', 'Cincinnati Children''s Hospital CF Center', '3333 Burnet Ave', 'Cincinnati', 'OH', '45229', '513-636-4200', 'cf@cchmc.org', 1, 'SYSTEM'),
(10, 'CF-010', 'Nationwide Children''s Hospital CF Center', '700 Children''s Dr', 'Columbus', 'OH', '43205', '614-722-2000', 'cf@nationwidechildrens.org', 1, 'SYSTEM');

SET IDENTITY_INSERT ngr.CarePrograms OFF;

-- =============================================
-- SEED SAMPLE FORM DEFINITIONS
-- =============================================

SET IDENTITY_INSERT ngr.FormDefinitions ON;

INSERT INTO ngr.FormDefinitions (Id, Code, Name, Description, Version, SchemaJson, EncounterTypeId, IsActive, EffectiveFrom, CreatedBy) VALUES
(1, 'ANNUAL_REVIEW', 'Annual Review Form', 'Comprehensive annual patient review', 1, 
'{"type":"object","properties":{"pulmonaryFunction":{"type":"object","properties":{"fev1":{"type":"number"},"fvc":{"type":"number"}}},"weight":{"type":"number"},"height":{"type":"number"}}}', 
1, 1, '2024-01-01', 'SYSTEM'),
(2, 'QUARTERLY_VISIT', 'Quarterly Visit Form', 'Quarterly follow-up visit', 1,
'{"type":"object","properties":{"weight":{"type":"number"},"symptoms":{"type":"array","items":{"type":"string"}},"medications":{"type":"array","items":{"type":"object"}}}}',
2, 1, '2024-01-01', 'SYSTEM'),
(3, 'HOSPITALIZATION', 'Hospitalization Form', 'Inpatient hospitalization details', 1,
'{"type":"object","properties":{"admissionDate":{"type":"string","format":"date"},"dischargeDate":{"type":"string","format":"date"},"reason":{"type":"string"},"complications":{"type":"array"}}}',
3, 1, '2024-01-01', 'SYSTEM');

SET IDENTITY_INSERT ngr.FormDefinitions OFF;

-- =============================================
-- SEED SAMPLE STANDARD REPORTS
-- =============================================

SET IDENTITY_INSERT ngr.StandardReports ON;

INSERT INTO ngr.StandardReports (Id, Code, Name, Description, Category, QueryDefinitionJson, ColumnsJson, RequiredRole, IsActive) VALUES
(1, 'PATIENT_ROSTER', 'Patient Roster', 'Active patients by program', 'Clinical',
'{"entity":"Patient","joins":["PatientPrograms","CarePrograms"],"filters":[{"field":"PatientPrograms.Status","operator":"eq","value":"ACTIVE"}]}',
'[{"field":"RegistryId","label":"Registry ID"},{"field":"FirstName","label":"First Name"},{"field":"LastName","label":"Last Name"},{"field":"DateOfBirth","label":"DOB"}]',
'ClinicalUser', 1),
(2, 'ENCOUNTER_SUMMARY', 'Encounter Summary', 'Encounters by type and year', 'Clinical',
'{"entity":"Encounter","joins":["EncounterTypes","CarePrograms"],"groupBy":["EncounterYear","EncounterTypeId"]}',
'[{"field":"EncounterYear","label":"Year"},{"field":"EncounterType.Name","label":"Type"},{"field":"COUNT(*)","label":"Count"}]',
'FoundationAnalyst', 1),
(3, 'FORM_COMPLETION', 'Form Completion Rate', 'Form submission status by program', 'Quality',
'{"entity":"FormSubmission","joins":["FormDefinitions","FormStatuses","CarePrograms"],"groupBy":["ProgramId","StatusId"]}',
'[{"field":"CareProgram.Name","label":"Program"},{"field":"FormStatus.Name","label":"Status"},{"field":"COUNT(*)","label":"Count"}]',
'ProgramAdmin', 1);

SET IDENTITY_INSERT ngr.StandardReports OFF;

-- =============================================
-- SEED SAMPLE CONTENT
-- =============================================

SET IDENTITY_INSERT ngr.Contents ON;

INSERT INTO ngr.Contents (Id, Title, Slug, Category, Body, IsPublished, PublishedAt, CreatedBy) VALUES
(1, 'Getting Started with NGR', 'getting-started', 'Help', 
'<h1>Welcome to the Next Generation Registry</h1><p>This guide will help you get started with the NGR system...</p>', 
1, GETUTCDATE(), 'SYSTEM'),
(2, 'How to Submit Forms', 'submit-forms', 'Help',
'<h1>Submitting eCRFs</h1><p>Follow these steps to submit electronic case report forms...</p>',
1, GETUTCDATE(), 'SYSTEM'),
(3, 'Data Quality Guidelines', 'data-quality', 'Documentation',
'<h1>Data Quality Standards</h1><p>Ensure high-quality data by following these guidelines...</p>',
1, GETUTCDATE(), 'SYSTEM');

SET IDENTITY_INSERT ngr.Contents OFF;

-- =============================================
-- SEED SAMPLE ANNOUNCEMENT
-- =============================================

SET IDENTITY_INSERT ngr.Announcements ON;

INSERT INTO ngr.Announcements (Id, Title, Message, Priority, TargetAudience, StartDate, EndDate, IsActive, CreatedBy) VALUES
(1, 'Welcome to NGR', 'The Next Generation Registry is now live! Please review the getting started guide.', 'HIGH', 'ALL', GETUTCDATE(), DATEADD(DAY, 30, GETUTCDATE()), 1, 'SYSTEM');

SET IDENTITY_INSERT ngr.Announcements OFF;

GO
