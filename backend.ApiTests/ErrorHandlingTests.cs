using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace backend.ApiTests
{
    /// <summary>
    /// Tests for error handling and edge cases across all API endpoints
    /// </summary>
    public class ErrorHandlingTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;

        public ErrorHandlingTests(TestWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
        }

        [Fact]
        public async Task RequirementApiHandlesInvalidData()
        {
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

            // Test with invalid CreatedBy
            invalidRequirement.Title = "Valid Title";
            invalidRequirement.CreatedBy = 0; // Invalid user ID
            response = await _client.PostAsJsonAsync("/api/requirement", invalidRequirement, _jsonOptions);
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task TestCaseApiHandlesInvalidSteps()
        {
            // Test with steps having null description
            var testCaseWithInvalidSteps = new TestCaseDto
            {
                Title = "Test Case with Invalid Steps",
                Description = "Test case for error handling",
                SuiteId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto
                    {
                        Description = "", // Invalid - use empty string instead of null
                        ExpectedResult = "Some result"
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/testcase", testCaseWithInvalidSteps, _jsonOptions);
            Assert.False(response.IsSuccessStatusCode);

            // Test with steps having empty expected result
            testCaseWithInvalidSteps.Steps[0].Description = "Valid description";
            testCaseWithInvalidSteps.Steps[0].ExpectedResult = ""; // Invalid
            response = await _client.PostAsJsonAsync("/api/testcase", testCaseWithInvalidSteps, _jsonOptions);
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UserApiHandlesInvalidEmail()
        {
            // Test with invalid email format
            var invalidUser = new UserDto
            {
                UserName = "testuser",
                Email = "invalid-email" // Invalid format
            };

            var response = await _client.PostAsJsonAsync("/api/user", invalidUser, _jsonOptions);
            Assert.False(response.IsSuccessStatusCode);

            // Test with duplicate email (create user first)
            var validUser = new UserDto
            {
                UserName = "validuser",
                Email = "valid@example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/user", validUser, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();

            // Try to create another user with same email
            var duplicateUser = new UserDto
            {
                UserName = "anotheruser",
                Email = "valid@example.com" // Duplicate email
            };

            var duplicateResponse = await _client.PostAsJsonAsync("/api/user", duplicateUser, _jsonOptions);
            Assert.False(duplicateResponse.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ApiHandlesLargeDataPayloads()
        {
            // Test with very long title
            var longTitle = new string('A', 10000); // Very long title
            var requirementWithLongTitle = new RequirementDto
            {
                Title = longTitle,
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Normal description",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var response = await _client.PostAsJsonAsync("/api/requirement", requirementWithLongTitle, _jsonOptions);
            // Should either succeed (if server accepts large titles) or fail gracefully
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);

            // Test with very long description
            var longDescription = new string('B', 50000);
            var requirementWithLongDesc = new RequirementDto
            {
                Title = "Normal Title",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = longDescription,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            response = await _client.PostAsJsonAsync("/api/requirement", requirementWithLongDesc, _jsonOptions);
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ApiHandlesMalformedJson()
        {
            // Test with malformed JSON
            var malformedJson = "{\"Title\": \"Test\", \"Type\": \"Invalid\", \"Status\":}"; // Missing value
            var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/requirement", content);
            Assert.False(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async Task ApiHandlesInvalidEnumValues()
        {
            // Test with invalid enum value in JSON
            var invalidEnumJson = @"{
                ""Title"": ""Test Requirement"",
                ""Type"": ""InvalidType"",
                ""Status"": ""Draft"",
                ""Description"": ""Test"",
                ""CreatedBy"": 1,
                ""CreatedAt"": ""2023-01-01T00:00:00Z""
            }";

            var content = new StringContent(invalidEnumJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/requirement", content);
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ApiHandlesCircularReferences()
        {
            // Create a parent requirement
            var parentReq = new RequirementDto
            {
                Title = "Parent Requirement",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Parent for circular reference test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var parentResponse = await _client.PostAsJsonAsync("/api/requirement", parentReq, _jsonOptions);
            parentResponse.EnsureSuccessStatusCode();
            var createdParent = await parentResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Create a child requirement
            var childReq = new RequirementDto
            {
                Title = "Child Requirement",
                Type = RequirementType.PRS,
                Status = RequirementStatus.Draft,
                Description = "Child for circular reference test",
                ParentId = createdParent?.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var childResponse = await _client.PostAsJsonAsync("/api/requirement", childReq, _jsonOptions);
            childResponse.EnsureSuccessStatusCode();
            var createdChild = await childResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Try to create circular reference by making parent a child of child
            if (createdParent != null && createdChild != null)
            {
                createdParent.ParentId = createdChild.Id;
                var circularResponse = await _client.PutAsJsonAsync($"/api/requirement/{createdParent.Id}", createdParent, _jsonOptions);
                
                // Should fail to prevent circular reference
                Assert.False(circularResponse.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task ApiHandlesConcurrentModification()
        {
            // Create a requirement
            var requirement = new RequirementDto
            {
                Title = "Concurrent Modification Test",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Test for concurrent modification",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", requirement, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            if (created != null)
            {
                // Simulate concurrent modification by making two updates with same base version
                var update1 = new RequirementDto
                {
                    Id = created.Id,
                    Title = "Update 1",
                    Type = created.Type,
                    Status = created.Status,
                    Description = created.Description,
                    CreatedBy = created.CreatedBy,
                    CreatedAt = created.CreatedAt,
                    Version = created.Version // Same version
                };

                var update2 = new RequirementDto
                {
                    Id = created.Id,
                    Title = "Update 2",
                    Type = created.Type,
                    Status = created.Status,
                    Description = created.Description,
                    CreatedBy = created.CreatedBy,
                    CreatedAt = created.CreatedAt,
                    Version = created.Version // Same version
                };

                // Both updates should succeed (last one wins) or handle optimistic concurrency
                var response1 = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", update1, _jsonOptions);
                var response2 = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", update2, _jsonOptions);

                // At least one should succeed
                Assert.True(response1.IsSuccessStatusCode || response2.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task ApiHandlesInvalidContentType()
        {
            // Test with XML content type (should expect JSON)
            var xmlContent = "<requirement><title>Test</title></requirement>";
            var content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

            var response = await _client.PostAsync("/api/requirement", content);
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Fact]
        public async Task ApiHandlesEmptyAndNullRequests()
        {
            // Test with empty body
            var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/requirement", emptyContent);
            Assert.False(response.IsSuccessStatusCode);

            // Test with null JSON
            var nullContent = new StringContent("null", Encoding.UTF8, "application/json");
            response = await _client.PostAsync("/api/requirement", nullContent);
            Assert.False(response.IsSuccessStatusCode);
        }
    }
}