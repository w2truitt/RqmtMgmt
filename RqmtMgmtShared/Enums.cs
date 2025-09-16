namespace RqmtMgmtShared
{
    /// <summary>
    /// Specifies the type of requirement: CRD, PRD, SRS, UserStory, BusinessRule, or EntityName.
    /// </summary>
    public enum RequirementType { CRD, PRD, SRS, UserStory, BusinessRule, EntityName }

    /// <summary>
    /// Specifies the type of requirement document (CRD, PRD, SRS).
    /// </summary>
    public enum DocumentType { CRD, PRD, SRS }

    /// <summary>
    /// Specifies the status of a document (Draft, InReview, Approved, Published).
    /// </summary>
    public enum DocumentStatus { Draft, InReview, Approved, Published }

    /// <summary>
    /// Specifies the type of trace relationship between requirements.
    /// </summary>
    public enum TraceType { DerivedFrom, ImplementedBy, ValidatedBy, RelatedTo }

    /// <summary>
    /// Specifies the status of a requirement (Draft, Approved, Implemented, Verified).
    /// </summary>
    public enum RequirementStatus { Draft, Approved, Implemented, Verified }

    /// <summary>
    /// Specifies the result of a test run.
    /// </summary>
    public enum TestResult { Passed, Failed, Blocked, NotRun }

    /// <summary>
    /// Specifies the type of test plan (UserValidation, SoftwareVerification).
    /// </summary>
    public enum TestPlanType { UserValidation, SoftwareVerification }

    /// <summary>
    /// Specifies the status of a test run session.
    /// </summary>
    public enum TestRunStatus { InProgress, Completed, Aborted, Paused }

    /// <summary>
    /// Specifies the status of a project.
    /// </summary>
    public enum ProjectStatus { Active, Archived, OnHold, Planning }

    /// <summary>
    /// Specifies the role of a user within a project.
    /// </summary>
    public enum ProjectRole { ProjectOwner, Developer, QAEngineer, ScrumMaster, BusinessAnalyst, Stakeholder, ProductOwner, Engineer, Tester }

    /// <summary>
    /// Specifies the priority level of a test case.
    /// </summary>
    public enum TestCasePriority { Critical, High, Medium, Low }
}