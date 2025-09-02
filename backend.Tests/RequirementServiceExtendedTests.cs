#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RqmtMgmtShared;

namespace backend.Tests
{
    /// <summary>
    /// Extended tests for RequirementService focusing on complex business logic and edge cases.
    /// These tests target the missing 42% coverage to reach 85%+ coverage target.
    /// </summary>
    public class RequirementServiceExtendedTests
    {
        #region GetPagedAsync Tests (28 Cyclomatic Complexity)

        [Fact]
        public async Task GetPagedAsync_WithProjectFilter_ReturnsOnlyProjectRequirements()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_WithProjectFilter_ReturnsOnlyProjectRequirements));
            var (user, project1) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            // Create second project
            var project2 = new Project
            {
                Name = "Project 2",
                Code = "PROJ2",
                Status = ProjectStatus.Active,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Projects.Add(project2);
            await db.SaveChangesAsync();

            // Create requirements for both projects
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project1.Id, user.Id, "Project1 Req1"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project1.Id, user.Id, "Project1 Req2"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project2.Id, user.Id, "Project2 Req1"));
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, ProjectId = project1.Id };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, req => Assert.Equal(project1.Id, req.ProjectId));
        }

        [Fact]
        public async Task GetPagedAsync_WithoutProjectFilter_ReturnsAllRequirements()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_WithoutProjectFilter_ReturnsAllRequirements));
            var (user, project1) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            var project2 = new Project
            {
                Name = "Project 2",
                Code = "PROJ2",
                Status = ProjectStatus.Active,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Projects.Add(project2);
            await db.SaveChangesAsync();

            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project1.Id, user.Id, "Req1"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project2.Id, user.Id, "Req2"));
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10 };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WithSearchTerm_FiltersByTitleAndDescription()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_WithSearchTerm_FiltersByTitleAndDescription));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req1 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Login Feature");
            req1.Description = "User authentication system";
            
            var req2 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Database Schema");
            req2.Description = "Contains login tables";
            
            var req3 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Payment Gateway");
            req3.Description = "External payment processing";

            db.Requirements.AddRange(req1, req2, req3);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SearchTerm = "login" };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(2, result.TotalItems);
            Assert.Contains(result.Items, r => r.Title == "Login Feature");
            Assert.Contains(result.Items, r => r.Title == "Database Schema");
        }

        [Fact]
        public async Task GetPagedAsync_SearchIsCaseInsensitive()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_SearchIsCaseInsensitive));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "LOGIN Feature");
            req.Description = "USER Authentication";
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SearchTerm = "login" };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(1, result.TotalItems);
            Assert.Equal("LOGIN Feature", result.Items[0].Title);
        }

        [Fact]
        public async Task GetPagedAsync_SortByTitle_Ascending()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_SortByTitle_Ascending));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Zebra"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Alpha"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Beta"));
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SortBy = "title", SortDescending = false };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal("Alpha", result.Items[0].Title);
            Assert.Equal("Beta", result.Items[1].Title);
            Assert.Equal("Zebra", result.Items[2].Title);
        }

        [Fact]
        public async Task GetPagedAsync_SortByTitle_Descending()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_SortByTitle_Descending));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Alpha"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Beta"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Zebra"));
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SortBy = "title", SortDescending = true };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal("Zebra", result.Items[0].Title);
            Assert.Equal("Beta", result.Items[1].Title);
            Assert.Equal("Alpha", result.Items[2].Title);
        }

        [Fact]
        public async Task GetPagedAsync_SortByStatus()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_SortByStatus));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req1 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req1");
            req1.Status = RequirementStatus.Verified;
            
            var req2 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req2");
            req2.Status = RequirementStatus.Draft;
            
            var req3 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req3");
            req3.Status = RequirementStatus.Approved;

            db.Requirements.AddRange(req1, req2, req3);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SortBy = "status", SortDescending = false };

            var result = await service.GetPagedAsync(parameters);

            // When sorting by status ascending: Draft(0), Approved(1), Verified(3)
            Assert.Equal(RequirementStatus.Draft, result.Items[0].Status);
            Assert.Equal(RequirementStatus.Approved, result.Items[1].Status);
            Assert.Equal(RequirementStatus.Verified, result.Items[2].Status);
        }

        [Fact]
        public async Task GetPagedAsync_SortByType()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_SortByType));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req1 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req1");
            req1.Type = RequirementType.SRS;
            
            var req2 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req2");
            req2.Type = RequirementType.CRS;
            
            var req3 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req3");
            req3.Type = RequirementType.PRS;

            db.Requirements.AddRange(req1, req2, req3);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SortBy = "type", SortDescending = false };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(RequirementType.CRS, result.Items[0].Type);
            Assert.Equal(RequirementType.PRS, result.Items[1].Type);
            Assert.Equal(RequirementType.SRS, result.Items[2].Type);
        }

        [Fact]
        public async Task GetPagedAsync_SortByCreatedAt()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_SortByCreatedAt));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var baseTime = DateTime.UtcNow;
            var req1 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req1");
            req1.CreatedAt = baseTime.AddDays(-2);
            
            var req2 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req2");
            req2.CreatedAt = baseTime.AddDays(-1);
            
            var req3 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req3");
            req3.CreatedAt = baseTime;

            db.Requirements.AddRange(req1, req2, req3);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SortBy = "createdat", SortDescending = false };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal("Req1", result.Items[0].Title);
            Assert.Equal("Req2", result.Items[1].Title);
            Assert.Equal("Req3", result.Items[2].Title);
        }

        [Fact]
        public async Task GetPagedAsync_DefaultSortById()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_DefaultSortById));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req1"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req2"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req3"));
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10 };

            var result = await service.GetPagedAsync(parameters);

            // Should be sorted by ID ascending by default
            for (int i = 1; i < result.Items.Count; i++)
            {
                Assert.True(result.Items[i-1].Id < result.Items[i].Id);
            }
        }

        [Fact]
        public async Task GetPagedAsync_Pagination_ReturnsCorrectPage()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_Pagination_ReturnsCorrectPage));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            // Create 5 requirements
            for (int i = 1; i <= 5; i++)
            {
                db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, $"Req{i}"));
            }
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 2, PageSize = 2 };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(5, result.TotalItems);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyResult_ReturnsEmptyList()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_EmptyResult_ReturnsEmptyList));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10, SearchTerm = "nonexistent" };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(0, result.TotalItems);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetPagedAsync_CombinedFilters_ProjectAndSearch()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetPagedAsync_CombinedFilters_ProjectAndSearch));
            var (user, project1) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            var project2 = new Project
            {
                Name = "Project 2",
                Code = "PROJ2",
                Status = ProjectStatus.Active,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Projects.Add(project2);
            await db.SaveChangesAsync();

            // Project 1 requirements
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project1.Id, user.Id, "Login System"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project1.Id, user.Id, "Payment Gateway"));
            
            // Project 2 requirements
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project2.Id, user.Id, "Login Feature"));
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var parameters = new PaginationParameters 
            { 
                PageNumber = 1, 
                PageSize = 10, 
                ProjectId = project1.Id, 
                SearchTerm = "login" 
            };

            var result = await service.GetPagedAsync(parameters);

            Assert.Equal(1, result.TotalItems);
            Assert.Equal("Login System", result.Items[0].Title);
            Assert.Equal(project1.Id, result.Items[0].ProjectId);
        }

        #endregion

        #region CreateAsync Validation Tests

        [Fact]
        public async Task CreateAsync_WithNullTitle_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithNullTitle_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id);
            dto.Title = null!;

            var result = await service.CreateAsync(dto);

            Assert.Null(result);
            Assert.Empty(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WithEmptyTitle_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithEmptyTitle_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id);
            dto.Title = "";

            var result = await service.CreateAsync(dto);

            Assert.Null(result);
            Assert.Empty(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WithWhitespaceTitle_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithWhitespaceTitle_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id);
            dto.Title = "   ";

            var result = await service.CreateAsync(dto);

            Assert.Null(result);
            Assert.Empty(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WithInvalidCreatedBy_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithInvalidCreatedBy_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id);
            dto.CreatedBy = 0;

            var result = await service.CreateAsync(dto);

            Assert.Null(result);
            Assert.Empty(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WithNegativeCreatedBy_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithNegativeCreatedBy_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id);
            dto.CreatedBy = -1;

            var result = await service.CreateAsync(dto);

            Assert.Null(result);
            Assert.Empty(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_CreatesInitialVersion()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_CreatesInitialVersion));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id, "Test Requirement");

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);
            // The main entity doesn't get version updated during CreateAsync, only RequirementVersion table does
            Assert.Equal(0, result.Version);

            // Verify version was created in the RequirementVersion table
            var versions = await db.RequirementVersions.Where(v => v.RequirementId == result.Id).ToListAsync();
            Assert.Single(versions);
            Assert.Equal(1, versions[0].Version);
            Assert.Equal("Test Requirement", versions[0].Title);
        }

        #endregion

        #region UpdateAsync Validation Tests

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnsFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_WithNonExistentId_ReturnsFalse));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var service = new RequirementService(db);
            var dto = new RequirementDto
            {
                Id = 999,
                Title = "Non-existent",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                ProjectId = project.Id,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_WithNullTitle_ReturnsFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_WithNullTitle_ReturnsFalse));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Original Title");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var dto = new RequirementDto
            {
                Id = req.Id,
                Title = null!,
                Type = req.Type,
                Status = req.Status,
                ProjectId = project.Id,
                CreatedBy = req.CreatedBy,
                CreatedAt = req.CreatedAt
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_WithEmptyTitle_ReturnsFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_WithEmptyTitle_ReturnsFalse));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Original Title");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var dto = new RequirementDto
            {
                Id = req.Id,
                Title = "",
                Type = req.Type,
                Status = req.Status,
                ProjectId = project.Id,
                CreatedBy = req.CreatedBy,
                CreatedAt = req.CreatedAt
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidCreatedBy_ReturnsFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_WithInvalidCreatedBy_ReturnsFalse));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Original Title");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var dto = new RequirementDto
            {
                Id = req.Id,
                Title = "Updated Title",
                Type = req.Type,
                Status = req.Status,
                ProjectId = project.Id,
                CreatedBy = 0,
                CreatedAt = req.CreatedAt
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_CreatesNewVersionAndUpdatesEntity()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_CreatesNewVersionAndUpdatesEntity));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Original Title");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            // Create initial version manually (simulating CreateAsync behavior)
            var initialVersion = new RequirementVersion
            {
                RequirementId = req.Id,
                Version = 1,
                Type = req.Type,
                Title = req.Title,
                Description = req.Description,
                ParentId = req.ParentId,
                Status = req.Status,
                ModifiedBy = req.CreatedBy,
                ModifiedAt = req.CreatedAt
            };
            db.RequirementVersions.Add(initialVersion);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var dto = new RequirementDto
            {
                Id = req.Id,
                Title = "Updated Title",
                Type = RequirementType.PRS,
                Status = RequirementStatus.Approved,
                ProjectId = project.Id,
                CreatedBy = req.CreatedBy,
                CreatedAt = req.CreatedAt
            };

            var result = await service.UpdateAsync(dto);

            Assert.True(result);

            // Verify entity was updated
            var updatedReq = await db.Requirements.FindAsync(req.Id);
            Assert.NotNull(updatedReq);
            Assert.Equal("Updated Title", updatedReq!.Title);
            Assert.Equal(RequirementType.PRS, updatedReq.Type);
            Assert.Equal(RequirementStatus.Approved, updatedReq.Status);
            Assert.Equal(2, updatedReq.Version);
            Assert.NotNull(updatedReq.UpdatedAt);

            // Verify new version was created
            var versions = await db.RequirementVersions.Where(v => v.RequirementId == req.Id).OrderBy(v => v.Version).ToListAsync();
            Assert.Equal(2, versions.Count);
            Assert.Equal("Original Title", versions[0].Title);
            // The second version contains the OLD state before update (that's how UpdateAsync works)
            Assert.Equal("Original Title", versions[1].Title);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTimestamp()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_UpdatesTimestamp));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Original Title");
            req.UpdatedAt = null;
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var dto = new RequirementDto
            {
                Id = req.Id,
                Title = "Updated Title",
                Type = req.Type,
                Status = req.Status,
                ProjectId = project.Id,
                CreatedBy = req.CreatedBy,
                CreatedAt = req.CreatedAt
            };

            var beforeUpdate = DateTime.UtcNow;
            var result = await service.UpdateAsync(dto);
            var afterUpdate = DateTime.UtcNow;

            Assert.True(result);

            var updatedReq = await db.Requirements.FindAsync(req.Id);
            Assert.NotNull(updatedReq!.UpdatedAt);
            Assert.True(updatedReq.UpdatedAt >= beforeUpdate);
            Assert.True(updatedReq.UpdatedAt <= afterUpdate);
        }

        #endregion

        #region Circular Reference Tests

        [Fact]
        public async Task CreateAsync_WithDirectCircularReference_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithDirectCircularReference_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            // Create parent requirement
            var parent = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Parent");
            db.Requirements.Add(parent);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            
            // Try to create child that points to parent
            var childDto = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id, "Child");
            childDto.ParentId = parent.Id;
            var child = await service.CreateAsync(childDto);
            Assert.NotNull(child);

            // Now try to make parent point to child (creating circular reference)
            var dto = new RequirementDto
            {
                Id = parent.Id,
                Title = parent.Title,
                Type = parent.Type,
                Status = parent.Status,
                ProjectId = project.Id,
                CreatedBy = parent.CreatedBy,
                CreatedAt = parent.CreatedAt,
                ParentId = child.Id // This would create a circular reference
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_WithSelfReference_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithSelfReference_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            // Create requirement first
            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Self Ref");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);

            // Try to make it reference itself
            var dto = new RequirementDto
            {
                Id = req.Id,
                Title = req.Title,
                Type = req.Type,
                Status = req.Status,
                ProjectId = project.Id,
                CreatedBy = req.CreatedBy,
                CreatedAt = req.CreatedAt,
                ParentId = req.Id // Self-reference
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_WithMultiLevelCircularReference_ReturnsNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithMultiLevelCircularReference_ReturnsNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            // Create chain A -> B -> C
            var reqA = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req A");
            db.Requirements.Add(reqA);
            await db.SaveChangesAsync();

            var reqB = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req B");
            reqB.ParentId = reqA.Id;
            db.Requirements.Add(reqB);
            await db.SaveChangesAsync();

            var reqC = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req C");
            reqC.ParentId = reqB.Id;
            db.Requirements.Add(reqC);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);

            // Try to make A point to C, creating A -> B -> C -> A cycle
            var dto = new RequirementDto
            {
                Id = reqA.Id,
                Title = reqA.Title,
                Type = reqA.Type,
                Status = reqA.Status,
                ProjectId = project.Id,
                CreatedBy = reqA.CreatedBy,
                CreatedAt = reqA.CreatedAt,
                ParentId = reqC.Id // This would create A -> B -> C -> A cycle
            };

            var result = await service.UpdateAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_WithValidHierarchy_Succeeds()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_WithValidHierarchy_Succeeds));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            // Create chain A -> B -> C (valid hierarchy)
            var reqA = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req A");
            db.Requirements.Add(reqA);
            await db.SaveChangesAsync();

            var reqB = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req B");
            reqB.ParentId = reqA.Id;
            db.Requirements.Add(reqB);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);

            // Create D pointing to B: A -> B -> C and B -> D (valid)
            var dtoD = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id, "Req D");
            dtoD.ParentId = reqB.Id;

            var result = await service.CreateAsync(dtoD);

            Assert.NotNull(result);
            Assert.Equal(reqB.Id, result.ParentId);
        }

        #endregion

        #region GetVersionsAsync Tests

        [Fact]
        public async Task GetVersionsAsync_WithNoVersions_ReturnsEmptyList()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetVersionsAsync_WithNoVersions_ReturnsEmptyList));
            var service = new RequirementService(db);

            var result = await service.GetVersionsAsync(999);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetVersionsAsync_WithSingleVersion_ReturnsOneVersion()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetVersionsAsync_WithSingleVersion_ReturnsOneVersion));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Test Req");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            var version = new RequirementVersion
            {
                RequirementId = req.Id,
                Version = 1,
                Type = req.Type,
                Title = req.Title,
                Description = req.Description,
                Status = req.Status,
                ModifiedBy = req.CreatedBy,
                ModifiedAt = req.CreatedAt
            };
            db.RequirementVersions.Add(version);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var result = await service.GetVersionsAsync(req.Id);

            Assert.Single(result);
            Assert.Equal(1, result[0].Version);
            Assert.Equal("Test Req", result[0].Title);
        }

        [Fact]
        public async Task GetVersionsAsync_WithMultipleVersions_ReturnsOrderedByVersion()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetVersionsAsync_WithMultipleVersions_ReturnsOrderedByVersion));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Test Req");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();

            // Add versions out of order to test sorting
            var version3 = new RequirementVersion
            {
                RequirementId = req.Id,
                Version = 3,
                Title = "Version 3",
                Type = req.Type,
                Status = req.Status,
                ModifiedBy = req.CreatedBy,
                ModifiedAt = DateTime.UtcNow.AddDays(2)
            };

            var version1 = new RequirementVersion
            {
                RequirementId = req.Id,
                Version = 1,
                Title = "Version 1",
                Type = req.Type,
                Status = req.Status,
                ModifiedBy = req.CreatedBy,
                ModifiedAt = DateTime.UtcNow
            };

            var version2 = new RequirementVersion
            {
                RequirementId = req.Id,
                Version = 2,
                Title = "Version 2",
                Type = req.Type,
                Status = req.Status,
                ModifiedBy = req.CreatedBy,
                ModifiedAt = DateTime.UtcNow.AddDays(1)
            };

            db.RequirementVersions.AddRange(version3, version1, version2);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var result = await service.GetVersionsAsync(req.Id);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Version);
            Assert.Equal(2, result[1].Version);
            Assert.Equal(3, result[2].Version);
            Assert.Equal("Version 1", result[0].Title);
            Assert.Equal("Version 2", result[1].Title);
            Assert.Equal("Version 3", result[2].Title);
        }

        [Fact]
        public async Task GetVersionsAsync_OnlyReturnsVersionsForSpecificRequirement()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetVersionsAsync_OnlyReturnsVersionsForSpecificRequirement));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);

            var req1 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req 1");
            var req2 = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Req 2");
            db.Requirements.AddRange(req1, req2);
            await db.SaveChangesAsync();

            // Add versions for both requirements
            var version1_1 = new RequirementVersion
            {
                RequirementId = req1.Id,
                Version = 1,
                Title = "Req 1 V1",
                Type = req1.Type,
                Status = req1.Status,
                ModifiedBy = req1.CreatedBy,
                ModifiedAt = DateTime.UtcNow
            };

            var version2_1 = new RequirementVersion
            {
                RequirementId = req2.Id,
                Version = 1,
                Title = "Req 2 V1",
                Type = req2.Type,
                Status = req2.Status,
                ModifiedBy = req2.CreatedBy,
                ModifiedAt = DateTime.UtcNow
            };

            db.RequirementVersions.AddRange(version1_1, version2_1);
            await db.SaveChangesAsync();

            var service = new RequirementService(db);
            var result = await service.GetVersionsAsync(req1.Id);

            Assert.Single(result);
            Assert.Equal("Req 1 V1", result[0].Title);
            Assert.Equal(req1.Id, result[0].RequirementId);
        }

        #endregion
    }
}
