using System;
using System.Collections.Generic;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for test cases containing test case information and associated test steps.
    /// </summary>
    public class TestCaseDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test case.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test suite this test case belongs to. Null for unassigned test cases.
        /// </summary>
        public int? SuiteId { get; set; }

        /// <summary>
        /// Gets or sets the title of the test case. This field is required and cannot be null.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the test case. Can be null for self-explanatory test cases.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the collection of test steps that define the test case execution procedure.
        /// </summary>
        public List<TestStepDto> Steps { get; set; } = new();

        /// <summary>
        /// Gets or sets the ID of the user who created this test case.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test case was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}