-- SQL script to reorganize document sections
-- This moves misplaced sections to their proper parent sections

USE RqmtMgmt;

-- First, let's check what we have
SELECT 'BEFORE REORGANIZATION - Parent Sections' as Status;
SELECT Id, Title, ParentSectionId, SectionOrder 
FROM DocumentSections 
WHERE DocumentId = 11 AND ParentSectionId IS NULL
ORDER BY SectionOrder;

-- Move main sections under "3. Functional Requirements" (ID: 2231)
UPDATE DocumentSections 
SET ParentSectionId = 2231, SectionOrder = 1, Level = 2
WHERE Id = 1 AND Title = '3.1 Authentication and Authorization';

UPDATE DocumentSections 
SET ParentSectionId = 2231, SectionOrder = 2, Level = 2
WHERE Id = 1434 AND Title = '3.2 User Management';

-- Move User Management subsections under "3.2 User Management" (ID: 1434)
UPDATE DocumentSections 
SET ParentSectionId = 1434, SectionOrder = 1, Level = 3
WHERE Id = 1333 AND Title = 'User Operations';

UPDATE DocumentSections 
SET ParentSectionId = 1434, SectionOrder = 2, Level = 3
WHERE Id = 1334 AND Title = 'User Roles';

-- Move duplicate User Operations and User Roles under proper parent
UPDATE DocumentSections 
SET ParentSectionId = 1434, SectionOrder = 1, Level = 3
WHERE Id = 1400 AND Title = 'User Operations';

UPDATE DocumentSections 
SET ParentSectionId = 1434, SectionOrder = 2, Level = 3
WHERE Id = 1401 AND Title = 'User Roles';

-- Move Project Management sections under "3. Functional Requirements"
-- First create 3.3 Project Management section
INSERT INTO DocumentSections (DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
VALUES (NULL, 2231, '3.3 Project Management', 'Project management requirements', 3, 2, '3.3', 0, GETDATE());

DECLARE @ProjectMgmtId INT = SCOPE_IDENTITY();

-- Move Project Operations and Project Segmentation under 3.3
UPDATE DocumentSections 
SET ParentSectionId = @ProjectMgmtId, SectionOrder = 1, Level = 3
WHERE Id = 1335 AND Title = 'Project Operations';

UPDATE DocumentSections 
SET ParentSectionId = @ProjectMgmtId, SectionOrder = 2, Level = 3
WHERE Id = 1336 AND Title = 'Project Segmentation';

-- Move duplicate Project Operations and Project Segmentation
UPDATE DocumentSections 
SET ParentSectionId = @ProjectMgmtId, SectionOrder = 1, Level = 3
WHERE Id = 1402 AND Title = 'Project Operations';

UPDATE DocumentSections 
SET ParentSectionId = @ProjectMgmtId, SectionOrder = 2, Level = 3
WHERE Id = 1403 AND Title = 'Project Segmentation';

-- Create 3.4 Requirements Management section
INSERT INTO DocumentSections (DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
VALUES (NULL, 2231, '3.4 Requirements Management', 'Requirements management functionality', 4, 2, '3.4', 0, GETDATE());

DECLARE @ReqMgmtId INT = SCOPE_IDENTITY();

-- Move Requirements Management sections under 3.4
UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 1, Level = 3
WHERE Id = 1337 AND Title = 'Requirement Types';

UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 2, Level = 3
WHERE Id = 1338 AND Title = 'Requirement Operations';

UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 3, Level = 3
WHERE Id = 1339 AND Title = 'Requirement Versioning';

UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 4, Level = 3
WHERE Id = 1340 AND Title = 'Requirement Traceability';

-- Move duplicate Requirements Management sections
UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 1, Level = 3
WHERE Id = 1404 AND Title = 'Requirement Types';

UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 2, Level = 3
WHERE Id = 1405 AND Title = 'Requirement Operations';

UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 3, Level = 3
WHERE Id = 1406 AND Title = 'Requirement Versioning';

UPDATE DocumentSections 
SET ParentSectionId = @ReqMgmtId, SectionOrder = 4, Level = 3
WHERE Id = 1407 AND Title = 'Requirement Traceability';

-- Create 4.1 Performance Requirements section under "4. Non-Functional Requirements" (ID: 2232)
INSERT INTO DocumentSections (DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
VALUES (NULL, 2232, '4.1 Performance Requirements', 'Performance and scalability requirements', 1, 2, '4.1', 0, GETDATE());

DECLARE @PerfReqId INT = SCOPE_IDENTITY();

-- Move Performance sections under 4.1
UPDATE DocumentSections 
SET ParentSectionId = @PerfReqId, SectionOrder = 1, Level = 3
WHERE Id = 1421 AND Title = 'Response Time';

UPDATE DocumentSections 
SET ParentSectionId = @PerfReqId, SectionOrder = 2, Level = 3
WHERE Id = 1422 AND Title = 'Throughput';

UPDATE DocumentSections 
SET ParentSectionId = @PerfReqId, SectionOrder = 3, Level = 3
WHERE Id = 1423 AND Title = 'Horizontal Scaling';

UPDATE DocumentSections 
SET ParentSectionId = @PerfReqId, SectionOrder = 4, Level = 3
WHERE Id = 1424 AND Title = 'Data Scaling';

UPDATE DocumentSections 
SET ParentSectionId = @PerfReqId, SectionOrder = 5, Level = 3
WHERE Id = 1425 AND Title = 'Availability';

UPDATE DocumentSections 
SET ParentSectionId = @PerfReqId, SectionOrder = 6, Level = 3
WHERE Id = 1426 AND Title = 'Data Integrity';

-- Create 4.2 Security Requirements section
INSERT INTO DocumentSections (DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
VALUES (NULL, 2232, '4.2 Security Requirements', 'Security and access control requirements', 2, 2, '4.2', 0, GETDATE());

DECLARE @SecReqId INT = SCOPE_IDENTITY();

-- Move Security sections under 4.2
UPDATE DocumentSections 
SET ParentSectionId = @SecReqId, SectionOrder = 1, Level = 3
WHERE Id = 1427 AND Title = 'Authentication Security';

UPDATE DocumentSections 
SET ParentSectionId = @SecReqId, SectionOrder = 2, Level = 3
WHERE Id = 1428 AND Title = 'Data Security';

UPDATE DocumentSections 
SET ParentSectionId = @SecReqId, SectionOrder = 3, Level = 3
WHERE Id = 1429 AND Title = 'Access Control';

-- Create 4.3 Quality Requirements section
INSERT INTO DocumentSections (DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
VALUES (NULL, 2232, '4.3 Quality Requirements', 'Code quality and maintainability requirements', 3, 2, '4.3', 0, GETDATE());

DECLARE @QualReqId INT = SCOPE_IDENTITY();

-- Move Quality sections under 4.3
UPDATE DocumentSections 
SET ParentSectionId = @QualReqId, SectionOrder = 1, Level = 3
WHERE Id = 1430 AND Title = 'Code Quality';

UPDATE DocumentSections 
SET ParentSectionId = @QualReqId, SectionOrder = 2, Level = 3
WHERE Id = 1431 AND Title = 'Testing';

UPDATE DocumentSections 
SET ParentSectionId = @QualReqId, SectionOrder = 3, Level = 3
WHERE Id = 1432 AND Title = 'Platform Independence';

UPDATE DocumentSections 
SET ParentSectionId = @QualReqId, SectionOrder = 4, Level = 3
WHERE Id = 1433 AND Title = 'Database Portability';

-- Check the results
SELECT 'AFTER REORGANIZATION - Hierarchical Structure' as Status;
SELECT 
    CASE 
        WHEN Level = 1 THEN Title
        WHEN Level = 2 THEN '  ' + Title  
        WHEN Level = 3 THEN '    ' + Title
        ELSE REPLICATE('  ', Level) + Title
    END as HierarchicalTitle,
    Id, 
    ParentSectionId, 
    Level,
    SectionOrder
FROM DocumentSections 
WHERE DocumentId = 11 OR ParentSectionId IN (
    SELECT Id FROM DocumentSections WHERE DocumentId = 11
) OR ParentSectionId IN (
    SELECT Id FROM DocumentSections WHERE ParentSectionId IN (
        SELECT Id FROM DocumentSections WHERE DocumentId = 11
    )
)
ORDER BY 
    CASE WHEN ParentSectionId IS NULL THEN SectionOrder ELSE 999 END,
    ParentSectionId,
    SectionOrder;

PRINT 'Reorganization completed successfully!';