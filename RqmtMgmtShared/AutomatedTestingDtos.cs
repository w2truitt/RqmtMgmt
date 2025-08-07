using RqmtMgmtShared;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for automated test results upload.
    /// Contains bulk test results from automated testing frameworks.
    /// </summary>
    public class AutomatedTestResultsDto
    {
        /// <summary>
        /// Gets or sets the name of the test framework or tool that generated these results.
        /// </summary>
        public string FrameworkName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the test framework.
        /// </summary>
        public string? FrameworkVersion { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the automated test run started.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the automated test run completed.
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the environment where the automated tests were executed.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the build version that was tested.
        /// </summary>
        public string? BuildVersion { get; set; }

        /// <summary>
        /// Gets or sets the collection of automated test case results.
        /// </summary>
        public List<AutomatedTestCaseResultDto> TestCaseResults { get; set; } = new List<AutomatedTestCaseResultDto>();
    }

    /// <summary>
    /// Data transfer object for individual automated test case result.
    /// Represents the result of a single automated test case execution.
    /// </summary>
    public class AutomatedTestCaseResultDto
    {
        /// <summary>
        /// Gets or sets the unique identifier or name of the test case in the automated framework.
        /// </summary>
        public string TestCaseIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the corresponding test case in the system (if mapped).
        /// </summary>
        public int? TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the name or title of the test case.
        /// </summary>
        public string TestCaseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result of the automated test execution.
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this test case was executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the duration of the test execution in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Gets or sets any error message if the test failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the stack trace if the test failed.
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or output from the test execution.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the collection of automated test step results (if available).
        /// </summary>
        public List<AutomatedTestStepResultDto> StepResults { get; set; } = new List<AutomatedTestStepResultDto>();
    }

    /// <summary>
    /// Data transfer object for individual automated test step result.
    /// Represents the result of a single step within an automated test case.
    /// </summary>
    public class AutomatedTestStepResultDto
    {
        /// <summary>
        /// Gets or sets the order of this step within the test case.
        /// </summary>
        public int StepOrder { get; set; }

        /// <summary>
        /// Gets or sets the description of the test step.
        /// </summary>
        public string StepDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result of this test step.
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the actual result observed during step execution.
        /// </summary>
        public string? ActualResult { get; set; }

        /// <summary>
        /// Gets or sets the duration of this step execution in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Gets or sets any error message if this step failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Data transfer object for test result upload summary.
    /// Provides feedback on the automated test result upload process.
    /// </summary>
    public class TestResultUploadSummaryDto
    {
        /// <summary>
        /// Gets or sets the total number of test case results that were processed.
        /// </summary>
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Gets or sets the number of test case results that were successfully uploaded.
        /// </summary>
        public int SuccessfulUploads { get; set; }

        /// <summary>
        /// Gets or sets the number of test case results that failed to upload.
        /// </summary>
        public int FailedUploads { get; set; }

        /// <summary>
        /// Gets or sets the number of test case results that were skipped (e.g., no matching test case).
        /// </summary>
        public int SkippedUploads { get; set; }

        /// <summary>
        /// Gets or sets the collection of errors encountered during upload.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the collection of warnings encountered during upload.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the timestamp when the upload was processed.
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating automated test sessions.
    /// Used when automated testing frameworks need to create test run sessions.
    /// </summary>
    public class CreateAutomatedTestSessionDto
    {
        /// <summary>
        /// Gets or sets the name of the automated test session.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the automated test session.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test plan to execute.
        /// </summary>
        public int TestPlanId { get; set; }

        /// <summary>
        /// Gets or sets the environment where tests will be executed.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the build version being tested.
        /// </summary>
        public string? BuildVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the automated testing framework.
        /// </summary>
        public string FrameworkName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the automated testing framework.
        /// </summary>
        public string? FrameworkVersion { get; set; }
    }

    /// <summary>
    /// Data transfer object for test results history with pagination.
    /// Provides historical test execution data with filtering capabilities.
    /// </summary>
    public class TestResultsHistoryDto
    {
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of records available.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages available.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the collection of test execution history records.
        /// </summary>
        public List<TestExecutionHistoryDto> Results { get; set; } = new List<TestExecutionHistoryDto>();
    }

    /// <summary>
    /// Data transfer object for individual test execution history record.
    /// Represents a single test execution in the historical view.
    /// </summary>
    public class TestExecutionHistoryDto
    {
        /// <summary>
        /// Gets or sets the test case execution ID.
        /// </summary>
        public int ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the test run session ID.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the test run session.
        /// </summary>
        public string SessionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the test case ID.
        /// </summary>
        public int TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the title of the test case.
        /// </summary>
        public string TestCaseTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the test plan ID.
        /// </summary>
        public int TestPlanId { get; set; }

        /// <summary>
        /// Gets or sets the name of the test plan.
        /// </summary>
        public string TestPlanName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result of the test execution.
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test was executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who executed the test.
        /// </summary>
        public string? ExecutorName { get; set; }

        /// <summary>
        /// Gets or sets the environment where the test was executed.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the build version that was tested.
        /// </summary>
        public string? BuildVersion { get; set; }

        /// <summary>
        /// Gets or sets any notes from the test execution.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the defect ID if the test failed.
        /// </summary>
        public string? DefectId { get; set; }
    }

    /// <summary>
    /// Data transfer object for generated test reports.
    /// Contains the report data and metadata for download.
    /// </summary>
    public class TestReportDto
    {
        /// <summary>
        /// Gets or sets the report data as byte array.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets the suggested filename for the report.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MIME content type of the report.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the report was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }
}