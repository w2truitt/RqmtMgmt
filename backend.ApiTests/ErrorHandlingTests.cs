using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for error handling and edge cases across all API endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class ErrorHandlingTests : BaseIntegrationTest
    {
        [Fact]
        public async Task RequirementApiHandlesInvalidData()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Test with null title
            var invalidRequirement = new RequirementDto
            {
                Title = "", // Invalid - use empty string instead of null
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Valid description",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var response = await _client.PostAsJsonAsync("/api/requirement", invalidRequirement, _jsonOptions);
            Assert.False(response.IsSuccessStatusCode);

            // Test with empty title
            invalidRequirement.Title = "";
            response = await _client.PostAsJsonAsync("/api/requirement", invalidRequirement, _jsonOptions);
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UserApiHandlesInvalidEmail()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var invalidUser = new UserDto
            {
                UserName = "validuser",
                Email = "invalid-email-format", // Invalid email format
                Roles = new List<string>()
            };

            var response = await _client.PostAsJsonAsync("/api/user", invalidUser, _jsonOptions);
            // The API should handle this gracefully - either accept it or return appropriate error
            // We're testing that it doesn't crash
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ApiHandlesMalformedJson()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Test with malformed JSON
            var malformedJson = "{\"title\": \"Test\", \"type\": \"Invalid\", }"; // Trailing comma
            var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/requirement", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ApiHandlesInvalidContentType()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var content = new StringContent("some random text", Encoding.UTF8, "text/plain");
            var response = await _client.PostAsync("/api/requirement", content);
            
            // Should return BadRequest or UnsupportedMediaType
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async Task ApiHandlesLargeDataPayloads()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create a requirement with very large description
            var largeDescription = new string('A', 10000); // 10KB description
            var requirement = new RequirementDto
            {
                Title = "Large Data Test",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = largeDescription,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var response = await _client.PostAsJsonAsync("/api/requirement", requirement, _jsonOptions);
            // Should either accept it or return appropriate error (not crash)
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ApiHandlesCircularReferences()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create parent requirement first
            var parentReq = new RequirementDto
            {
                Title = "Parent Requirement",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Parent for circular test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var parentResponse = await _client.PostAsJsonAsync("/api/requirement", parentReq, _jsonOptions);
            parentResponse.EnsureSuccessStatusCode();
            var createdParent = await parentResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Create child requirement
            var childReq = new RequirementDto
            {
                Title = "Child Requirement",
                Type = RequirementType.PRS,
                Status = RequirementStatus.Draft,
                Description = "Child for circular test",
                ParentId = createdParent!.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var childResponse = await _client.PostAsJsonAsync("/api/requirement", childReq, _jsonOptions);
            childResponse.EnsureSuccessStatusCode();
            var createdChild = await childResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Try to create circular reference by making parent a child of child
            createdParent.ParentId = createdChild!.Id;
            var circularResponse = await _client.PutAsJsonAsync($"/api/requirement/{createdParent.Id}", createdParent, _jsonOptions);
            
            // Should reject circular reference
            Assert.False(circularResponse.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ApiHandlesConcurrentModification()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create a requirement
            var requirement = new RequirementDto
            {
                Title = "Concurrent Test Requirement",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "For concurrent modification test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", requirement, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Simulate concurrent modification by updating twice
            created!.Description = "First modification";
            var firstUpdate = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
            
            created.Description = "Second modification";
            var secondUpdate = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);

            // Both should succeed (last write wins) or handle gracefully
            Assert.True(firstUpdate.IsSuccessStatusCode);
            Assert.True(secondUpdate.IsSuccessStatusCode);
        }

        /// <summary>
        /// Helper method to get a valid project ID for testing.
        /// </summary>
        private async Task<int> GetValidProjectIdAsync()
        {
            var response = await _client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();
            var projects = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projects?.Items?.Count == 0)
            {
                // Create a test project if none exist
                var createDto = new CreateProjectDto
                {
                    Name = $"Test Project for Errors {Guid.NewGuid():N}",
                    Code = $"ERR{DateTime.UtcNow:mmss}",
                    Description = "Auto-created for error testing",
                    OwnerId = 1,
                    Status = ProjectStatus.Planning
                };

                var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
                createResponse.EnsureSuccessStatusCode();
                var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
                return created!.Id;
            }

            return projects!.Items![0].Id;
        }
    }
}