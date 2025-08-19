using backend.Models;
using System;
using System.Linq;
using Xunit;

namespace backend.Tests.Models
{
    /// <summary>
    /// Contains unit tests for the TestSuite model.
    /// These tests focus on property accessors and collection initialization
    /// to close coverage gaps not addressed by service-level integration tests.
    /// </summary>
    public class TestSuiteTests
    {
        /// <summary>
        /// Test Case: Verifies that a new TestSuite instance has a non-null, empty TestCases collection.
        /// Hypothesis: The constructor must initialize the TestCases collection to prevent NullReferenceExceptions
        /// when items are added to a new instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesTestCases_ToEmptyList()
        {
            // Arrange & Act
            var testSuite = new TestSuite { Name = "Test Suite" };

            // Assert
            Assert.NotNull(testSuite.TestCases);
            Assert.Empty(testSuite.TestCases);
        }

        /// <summary>
        /// Test Case: Verifies that all scalar properties of the TestSuite model can be correctly set and retrieved.
        /// Hypothesis: Standard property getters and setters must function as expected to maintain data integrity.
        /// This test covers properties that are often set but not always read back in service-level tests.
        /// </summary>
        [Fact]
        public void ScalarProperties_CanBeSetAndGet()
        {
            // Arrange
            var testSuite = new TestSuite { Name = "Initial Name" };
            var now = DateTime.UtcNow;
            var name = "Login Functionality Test Suite";
            var description = "Tests all aspects of user login.";

            // Act
            testSuite.Id = 101;
            testSuite.Name = name;
            testSuite.Description = description;
            testSuite.CreatedBy = 42;
            testSuite.CreatedAt = now;
            testSuite.ProjectId = 99;

            // Assert
            Assert.Equal(101, testSuite.Id);
            Assert.Equal(name, testSuite.Name);
            Assert.Equal(description, testSuite.Description);
            Assert.Equal(42, testSuite.CreatedBy);
            Assert.Equal(now, testSuite.CreatedAt);
            Assert.Equal(99, testSuite.ProjectId);
        }

        /// <summary>
        /// Test Case: Verifies that navigation properties (Creator, Project) can be assigned and retrieved.
        /// Hypothesis: EF Core navigation properties should behave like regular object references, allowing
        /// related entities to be linked in memory.
        /// </summary>
        [Fact]
        public void NavigationProperties_CanBeSetAndGet()
        {
            // Arrange
            var testSuite = new TestSuite { Name = "Test Suite" };
            var creator = new User { Id = 42, UserName = "testuser", Email = "test@example.com" };
            var project = new Project { Id = 99, Name = "Test Project", Description = "Test project description", Code = "TEST001" };

            // Act
            testSuite.Creator = creator;
            testSuite.Project = project;

            // Assert
            Assert.Same(creator, testSuite.Creator);
            Assert.Same(project, testSuite.Project);
        }

        /// <summary>
        /// Test Case: Verifies that the TestCases collection property can be manipulated.
        /// Hypothesis: The underlying collection should allow adding and storing TestCase objects,
        /// confirming that it is a mutable collection.
        /// </summary>
        [Fact]
        public void TestCasesCollection_CanBeManipulated()
        {
            // Arrange
            var testSuite = new TestSuite { Name = "Test Suite" };
            var testCase1 = new TestCase { Id = 1, Title = "Test Case 1", Description = "First test case", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var testCase2 = new TestCase { Id = 2, Title = "Test Case 2", Description = "Second test case", CreatedBy = 1, CreatedAt = DateTime.UtcNow };

            // Act
            testSuite.TestCases.Add(testCase1);
            testSuite.TestCases.Add(testCase2);

            // Assert
            Assert.Equal(2, testSuite.TestCases.Count);
            Assert.Contains(testCase1, testSuite.TestCases);
            Assert.Contains(testCase2, testSuite.TestCases);
        }

        /// <summary>
        /// Test Case: Verifies that the TestCases collection can be enumerated.
        /// This ensures the collection implementation supports iteration.
        /// </summary>
        [Fact]
        public void TestCasesCollection_CanBeEnumerated()
        {
            // Arrange
            var testSuite = new TestSuite { Name = "Test Suite" };
            var testCase1 = new TestCase { Id = 1, Title = "Test Case 1", Description = "First test case", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var testCase2 = new TestCase { Id = 2, Title = "Test Case 2", Description = "Second test case", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            testSuite.TestCases.Add(testCase1);
            testSuite.TestCases.Add(testCase2);

            // Act
            var testCases = testSuite.TestCases.ToList();

            // Assert
            Assert.Equal(2, testCases.Count);
            Assert.Equal(testCase1.Title, testCases[0].Title);
            Assert.Equal(testCase2.Title, testCases[1].Title);
        }

        /// <summary>
        /// Test Case: Verifies that the TestSuite can be created with required properties.
        /// This tests the required Name property behavior.
        /// </summary>
        [Fact]
        public void TestSuite_CanBeCreatedWithRequiredName()
        {
            // Arrange & Act
            var testSuite = new TestSuite
            {
                Name = "Required Name Test Suite"
            };

            // Assert
            Assert.Equal("Required Name Test Suite", testSuite.Name);
            Assert.NotNull(testSuite.TestCases);
        }
    }
}
