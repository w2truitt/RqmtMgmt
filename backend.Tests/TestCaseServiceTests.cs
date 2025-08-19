#nullable enable
using System;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RqmtMgmtShared;
using System.Linq;
using System.Collections.Generic;

namespace backend.Tests
{
    public class TestCaseServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCaseServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsTestCase()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsTestCase));
            var service = new TestCaseService(db);
            var testCase = new TestCaseDto { Title = "Test Case", Description = "Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(testCase);
            
            Assert.NotNull(result);
            Assert.Equal("Test Case", result.Title);
            Assert.Single(await db.TestCases.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestCases()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestCases));
            db.TestCases.Add(new TestCase { Title = "TC1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            db.TestCases.Add(new TestCase { Title = "TC2", Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var all = await service.GetAllAsync();
            
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestCaseOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestCaseOrNull));
            var testCase = new TestCase { Title = "TC1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var found = await service.GetByIdAsync(testCase.Id);
            Assert.NotNull(found);
            Assert.Equal(testCase.Title, found!.Title);
            
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestCase()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestCase));
            var testCase = new TestCase { Title = "Old", Description = "Old Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var dto = new TestCaseDto { Id = testCase.Id, Title = "New", Description = "New Desc", CreatedBy = testCase.CreatedBy, CreatedAt = testCase.CreatedAt };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var testCase = new TestCase { Title = "ToDelete", Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var ok = await service.DeleteAsync(testCase.Id);
            Assert.True(ok);
            Assert.Empty(await db.TestCases.ToListAsync());
            
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }

        #region CreateAsync Validation Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_WithInvalidTitle_ReturnsNull(string? invalidTitle)
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_WithInvalidTitle_ReturnsNull) + invalidTitle?.Replace(" ", "_"));
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Title = invalidTitle!,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto { Description = "Step 1", ExpectedResult = "Result 1" }
                }
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, await db.TestCases.CountAsync());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task CreateAsync_WithInvalidCreatedBy_ReturnsNull(int invalidUserId)
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_WithInvalidCreatedBy_ReturnsNull) + invalidUserId);
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Title = "Valid Title",
                CreatedBy = invalidUserId,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto { Description = "Step 1", ExpectedResult = "Result 1" }
                }
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, await db.TestCases.CountAsync());
        }

        [Theory]
        [InlineData(null, "Valid Result")]
        [InlineData("", "Valid Result")]
        [InlineData("   ", "Valid Result")]
        [InlineData("Valid Description", null)]
        [InlineData("Valid Description", "")]
        [InlineData("Valid Description", "   ")]
        public async Task CreateAsync_WithInvalidStepDetails_ReturnsNull(string? description, string? expectedResult)
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_WithInvalidStepDetails_ReturnsNull) + 
                (description?.Replace(" ", "_") ?? "null") + "_" + (expectedResult?.Replace(" ", "_") ?? "null"));
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Title = "Valid Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto { Description = description!, ExpectedResult = expectedResult! }
                }
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, await db.TestCases.CountAsync());
        }

        [Fact]
        public async Task CreateAsync_WithNullSteps_Succeeds()
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_WithNullSteps_Succeeds));
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Title = "Valid Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            dto.Steps = null!; // Explicitly test null assignment

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Valid Title", result.Title);
            Assert.Equal(1, await db.TestCases.CountAsync());
        }

        [Fact]
        public async Task CreateAsync_WithEmptySteps_Succeeds()
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_WithEmptySteps_Succeeds));
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Title = "Valid Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>()
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Valid Title", result.Title);
            Assert.Empty(result.Steps);
            Assert.Equal(1, await db.TestCases.CountAsync());
        }

        [Fact]
        public async Task CreateAsync_WithMultipleValidSteps_Succeeds()
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_WithMultipleValidSteps_Succeeds));
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Title = "Valid Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto { Description = "Step 1", ExpectedResult = "Result 1" },
                    new TestStepDto { Description = "Step 2", ExpectedResult = "Result 2" }
                }
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Valid Title", result.Title);
            Assert.Equal(2, result.Steps.Count);
            Assert.Equal(1, await db.TestCases.CountAsync());
            var testCaseInDb = await db.TestCases.Include(tc => tc.Steps).FirstAsync();
            Assert.Equal(2, testCaseInDb.Steps.Count);
        }

        #endregion

        #region UpdateAsync Validation Tests

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithNonExistentId_ReturnsFalse));
            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 999,
                Title = "Some Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_WithInvalidTitle_ReturnsFalseAndDoesNotUpdate(string? invalidTitle)
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithInvalidTitle_ReturnsFalseAndDoesNotUpdate) + invalidTitle?.Replace(" ", "_"));
            var originalTestCase = new TestCase 
            { 
                Id = 1, 
                Title = "Original Title", 
                CreatedBy = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            db.TestCases.Add(originalTestCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 1,
                Title = invalidTitle!,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.False(result);
            var testCaseInDb = await db.TestCases.AsNoTracking().FirstAsync(tc => tc.Id == 1);
            Assert.Equal("Original Title", testCaseInDb.Title);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task UpdateAsync_WithInvalidCreatedBy_ReturnsFalseAndDoesNotUpdate(int invalidUserId)
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithInvalidCreatedBy_ReturnsFalseAndDoesNotUpdate) + invalidUserId);
            var originalTestCase = new TestCase 
            { 
                Id = 1, 
                Title = "Original Title", 
                CreatedBy = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            db.TestCases.Add(originalTestCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 1,
                Title = "Updated Title",
                CreatedBy = invalidUserId,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.False(result);
            var testCaseInDb = await db.TestCases.AsNoTracking().FirstAsync(tc => tc.Id == 1);
            Assert.Equal("Original Title", testCaseInDb.Title);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidStepDetails_ReturnsFalseAndDoesNotUpdate()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithInvalidStepDetails_ReturnsFalseAndDoesNotUpdate));
            var originalTestCase = new TestCase
            {
                Id = 1,
                Title = "Original Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep> 
                { 
                    new TestStep { Description = "Original Step", ExpectedResult = "Original Result" } 
                }
            };
            db.TestCases.Add(originalTestCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 1,
                Title = "Updated Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto { Description = "", ExpectedResult = "Some Result" } // Invalid description
                }
            };

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.False(result);
            var testCaseInDb = await db.TestCases.Include(tc => tc.Steps).AsNoTracking().FirstAsync(tc => tc.Id == 1);
            Assert.Single(testCaseInDb.Steps);
            Assert.Equal("Original Step", testCaseInDb.Steps.First().Description);
            Assert.Equal("Original Title", testCaseInDb.Title);
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_CorrectlyReplacesSteps()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithValidData_CorrectlyReplacesSteps));
            var originalTestCase = new TestCase
            {
                Id = 1,
                Title = "Original Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>
                {
                    new TestStep { Description = "Old Step 1", ExpectedResult = "Old Result 1" },
                    new TestStep { Description = "Old Step 2", ExpectedResult = "Old Result 2" }
                }
            };
            db.TestCases.Add(originalTestCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 1,
                Title = "Updated Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto { Description = "New Step 1", ExpectedResult = "New Result 1" }
                }
            };

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.True(result);
            var updatedTestCase = await db.TestCases.Include(tc => tc.Steps).AsNoTracking().FirstAsync(tc => tc.Id == 1);
            Assert.Equal("Updated Title", updatedTestCase.Title);
            Assert.Single(updatedTestCase.Steps);
            Assert.Equal("New Step 1", updatedTestCase.Steps.First().Description);
            Assert.Equal("New Result 1", updatedTestCase.Steps.First().ExpectedResult);
        }

        [Fact]
        public async Task UpdateAsync_WithNullSteps_ClearsAllSteps()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithNullSteps_ClearsAllSteps));
            var originalTestCase = new TestCase
            {
                Id = 1,
                Title = "Original Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>
                {
                    new TestStep { Description = "Step 1", ExpectedResult = "Result 1" },
                    new TestStep { Description = "Step 2", ExpectedResult = "Result 2" }
                }
            };
            db.TestCases.Add(originalTestCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 1,
                Title = "Updated Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            dto.Steps = null!; // Explicitly test null assignment

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.True(result);
            var updatedTestCase = await db.TestCases.Include(tc => tc.Steps).AsNoTracking().FirstAsync(tc => tc.Id == 1);
            Assert.Equal("Updated Title", updatedTestCase.Title);
            Assert.Empty(updatedTestCase.Steps);
        }

        [Fact]
        public async Task UpdateAsync_WithEmptySteps_ClearsAllSteps()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_WithEmptySteps_ClearsAllSteps));
            var originalTestCase = new TestCase
            {
                Id = 1,
                Title = "Original Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>
                {
                    new TestStep { Description = "Step 1", ExpectedResult = "Result 1" }
                }
            };
            db.TestCases.Add(originalTestCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);
            var dto = new TestCaseDto
            {
                Id = 1,
                Title = "Updated Title",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>()
            };

            // Act
            var result = await service.UpdateAsync(dto);

            // Assert
            Assert.True(result);
            var updatedTestCase = await db.TestCases.Include(tc => tc.Steps).AsNoTracking().FirstAsync(tc => tc.Id == 1);
            Assert.Equal("Updated Title", updatedTestCase.Title);
            Assert.Empty(updatedTestCase.Steps);
        }

        #endregion

        #region DeleteAsync Enhanced Tests

        [Fact]
        public async Task DeleteAsync_WithExistingTestCase_CascadeDeletesSteps()
        {
            // Arrange
            using var db = GetDbContext(nameof(DeleteAsync_WithExistingTestCase_CascadeDeletesSteps));
            var testCase = new TestCase
            {
                Title = "Test Case To Delete",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>
                {
                    new TestStep { Description = "Step 1", ExpectedResult = "Result 1" },
                    new TestStep { Description = "Step 2", ExpectedResult = "Result 2" }
                }
            };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);

            // Act
            var result = await service.DeleteAsync(testCase.Id);

            // Assert
            Assert.True(result);
            Assert.Equal(0, await db.TestCases.CountAsync());
            Assert.Equal(0, await db.TestSteps.CountAsync()); // Verify cascade delete
        }

        #endregion

        #region GetByIdAsync Enhanced Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsTestCaseWithSteps()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetByIdAsync_WithValidId_ReturnsTestCaseWithSteps));
            var testCase = new TestCase
            {
                Title = "Test Case",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>
                {
                    new TestStep { Description = "Step 1", ExpectedResult = "Result 1" },
                    new TestStep { Description = "Step 2", ExpectedResult = "Result 2" }
                }
            };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);

            // Act
            var result = await service.GetByIdAsync(testCase.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Case", result.Title);
            Assert.Equal(2, result.Steps.Count);
            Assert.Equal("Step 1", result.Steps[0].Description);
            Assert.Equal("Step 2", result.Steps[1].Description);
        }

        #endregion

        #region GetAllAsync Enhanced Tests

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetAllAsync_WithEmptyDatabase_ReturnsEmptyList));
            var service = new TestCaseService(db);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WithTestCases_ReturnsAllWithSteps()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetAllAsync_WithTestCases_ReturnsAllWithSteps));
            var testCase1 = new TestCase
            {
                Title = "Test Case 1",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>
                {
                    new TestStep { Description = "Step 1", ExpectedResult = "Result 1" }
                }
            };
            var testCase2 = new TestCase
            {
                Title = "Test Case 2",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStep>()
            };
            db.TestCases.AddRange(testCase1, testCase2);
            await db.SaveChangesAsync();

            var service = new TestCaseService(db);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var tc1 = result.First(tc => tc.Title == "Test Case 1");
            var tc2 = result.First(tc => tc.Title == "Test Case 2");
            Assert.Single(tc1.Steps);
            Assert.Empty(tc2.Steps);
        }

        #endregion
    }
}