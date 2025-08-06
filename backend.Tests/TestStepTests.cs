using backend.Models;
using Xunit;

namespace backend.Tests
{
    public class TestStepTests
    {
        [Fact]
        public void TestStep_DefaultConstructor_InitializesProperties()
        {
            // Act
            var testStep = new TestStep();

            // Assert
            Assert.Equal(0, testStep.Id);
            Assert.Equal(0, testStep.TestCaseId);
            Assert.Equal(string.Empty, testStep.Description);
            Assert.Equal(string.Empty, testStep.ExpectedResult);
            Assert.Null(testStep.TestCase);
        }

        [Fact]
        public void TestStep_SetProperties_PropertiesSetCorrectly()
        {
            // Arrange
            var testStep = new TestStep();

            // Act
            testStep.Id = 1;
            testStep.TestCaseId = 100;
            testStep.Description = "Click the login button";
            testStep.ExpectedResult = "User should be logged in successfully";

            // Assert
            Assert.Equal(1, testStep.Id);
            Assert.Equal(100, testStep.TestCaseId);
            Assert.Equal("Click the login button", testStep.Description);
            Assert.Equal("User should be logged in successfully", testStep.ExpectedResult);
        }

        [Fact]
        public void TestStep_NavigationProperty_CanBeSet()
        {
            // Arrange
            var testStep = new TestStep();
            var testCase = new TestCase { Id = 100, Title = "Test Case" };

            // Act
            testStep.TestCase = testCase;

            // Assert
            Assert.Equal(testCase, testStep.TestCase);
        }

        [Fact]
        public void TestStep_StringProperties_DefaultToEmptyString()
        {
            // Act
            var testStep = new TestStep();

            // Assert
            Assert.Equal(string.Empty, testStep.Description);
            Assert.Equal(string.Empty, testStep.ExpectedResult);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Simple step description")]
        [InlineData("Complex step description with multiple actions and validations")]
        [InlineData("Step with special characters: @#$%^&*()")]
        public void TestStep_Description_AcceptsVariousStrings(string description)
        {
            // Arrange
            var testStep = new TestStep();

            // Act
            testStep.Description = description;

            // Assert
            Assert.Equal(description, testStep.Description);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Simple expected result")]
        [InlineData("Complex expected result with multiple conditions")]
        [InlineData("Expected result with numbers: 123 and symbols: !@#")]
        public void TestStep_ExpectedResult_AcceptsVariousStrings(string expectedResult)
        {
            // Arrange
            var testStep = new TestStep();

            // Act
            testStep.ExpectedResult = expectedResult;

            // Assert
            Assert.Equal(expectedResult, testStep.ExpectedResult);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        public void TestStep_TestCaseId_AcceptsValidIds(int testCaseId)
        {
            // Arrange
            var testStep = new TestStep();

            // Act
            testStep.TestCaseId = testCaseId;

            // Assert
            Assert.Equal(testCaseId, testStep.TestCaseId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        public void TestStep_Id_AcceptsValidIds(int id)
        {
            // Arrange
            var testStep = new TestStep();

            // Act
            testStep.Id = id;

            // Assert
            Assert.Equal(id, testStep.Id);
        }

        [Fact]
        public void TestStep_CanSetAllPropertiesAtOnce()
        {
            // Arrange
            var testCase = new TestCase { Id = 50, Title = "Parent Test Case" };

            // Act
            var testStep = new TestStep
            {
                Id = 25,
                TestCaseId = 50,
                Description = "Enter username and password",
                ExpectedResult = "Credentials should be accepted",
                TestCase = testCase
            };

            // Assert
            Assert.Equal(25, testStep.Id);
            Assert.Equal(50, testStep.TestCaseId);
            Assert.Equal("Enter username and password", testStep.Description);
            Assert.Equal("Credentials should be accepted", testStep.ExpectedResult);
            Assert.Equal(testCase, testStep.TestCase);
        }

        [Fact]
        public void TestStep_NavigationProperty_CanBeSetToNull()
        {
            // Arrange
            var testStep = new TestStep();
            var testCase = new TestCase { Id = 100, Title = "Test Case" };

            // Act - Set then clear
            testStep.TestCase = testCase;
            testStep.TestCase = null;

            // Assert
            Assert.Null(testStep.TestCase);
        }
    }
}