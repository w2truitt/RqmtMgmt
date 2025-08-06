namespace RqmtMgmtShared
{
    /// <summary>
    /// Specifies the type of requirement: CRS, PRS, SRS, UserStory, BusinessRule, or EntityName.
    /// </summary>
    public enum RequirementType { CRS, PRS, SRS, UserStory, BusinessRule, EntityName }

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
}