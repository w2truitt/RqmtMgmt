using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests that test complex workflows across multiple API controllers
    /// </summary>
    public class IntegrationWorkflowTests : BaseApiTest
    {
        public IntegrationWorkflowTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CompleteRequirementToTestWorkflow()
        {
            // 1. Create a requirement
            var requirementDto = new RequirementDto
            {
                Title = "Integration Test Requirement",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "A requirement for integration testing",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var reqResponse = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
            reqResponse.EnsureSuccessStatusCode();
            var createdRequirement = await reqResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(createdRequirement);

            // 2. Create a test suite
            var testSuiteDto = new TestSuiteDto
            {
                Name = "Integration Test Suite",
                Description = "Test suite for integration testing",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var suiteResponse = await _client.PostAsJsonAsync("/api/testsuite", testSuiteDto, _jsonOptions);
            suiteResponse.EnsureSuccessStatusCode();
            var createdSuite = await suiteResponse.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            Assert.NotNull(createdSuite);

            // 3. Create a test case in the suite
            var testCaseDto = new TestCaseDto
            {
                Title = "Integration Test Case",
                Description = "Test case for integration testing",
                SuiteId = createdSuite.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = new List<TestStepDto>
                {
                    new TestStepDto
                    {
                        Description = "Step 1: Verify requirement",
                        ExpectedResult = "Requirement is verified"
                    },
                    new TestStepDto
                    {
                        Description = "Step 2: Execute test",
                        ExpectedResult = "Test passes"
                    }
                }
            };

            var caseResponse = await _client.PostAsJsonAsync("/api/testcase", testCaseDto, _jsonOptions);
            caseResponse.EnsureSuccessStatusCode();
            var createdTestCase = await caseResponse.Content.ReadFromJsonAsync<TestCaseDto>(_jsonOptions);
            Assert.NotNull(createdTestCase);
            Assert.Equal(2, createdTestCase.Steps.Count);

            // 4. Link the requirement to the test case
            var linkResponse = await _client.PostAsync($"/api/requirementtestcaselink/{createdRequirement.Id}/{createdTestCase.Id}", null);
            linkResponse.EnsureSuccessStatusCode();

            // 5. Verify the link exists
            var linksResponse = await _client.GetAsync($"/api/requirementtestcaselink/requirement/{createdRequirement.Id}");
            linksResponse.EnsureSuccessStatusCode();
            var links = await linksResponse.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>(_jsonOptions);
            Assert.NotNull(links);
            Assert.Single(links);
            Assert.Equal(createdTestCase.Id, links[0].TestCaseId);

            // 6. Create a test plan and add the test case
            var testPlanDto = new TestPlanDto
            {
                Name = "Integration Test Plan",
                Description = "Test plan for integration testing",
                Type = "UserValidation",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var planResponse = await _client.PostAsJsonAsync("/api/testplan", testPlanDto, _jsonOptions);
            planResponse.EnsureSuccessStatusCode();
            var createdPlan = await planResponse.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(createdPlan);

            // 7. Update the requirement status
            createdRequirement.Status = RequirementStatus.Approved;
            var updateResponse = await _client.PutAsJsonAsync($"/api/requirement/{createdRequirement.Id}", createdRequirement, _jsonOptions);
            updateResponse.EnsureSuccessStatusCode();

            // 8. Verify requirement version history
            var versionsResponse = await _client.GetAsync($"/api/Redline/requirement/{createdRequirement.Id}/versions");
            versionsResponse.EnsureSuccessStatusCode();
            var versions = await versionsResponse.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
            Assert.NotNull(versions);
            Assert.True(versions.Count >= 2); // Initial + updated
        }

        [Fact]
        public async Task UserRoleManagementWorkflow()
        {
            // 1. Create a new user
            var userDto = new UserDto
            {
                UserName = "roletest",
                Email = "roletest@example.com"
            };

            var userResponse = await _client.PostAsJsonAsync("/api/user", userDto, _jsonOptions);
            userResponse.EnsureSuccessStatusCode();
            var createdUser = await userResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
            Assert.NotNull(createdUser);

            // 2. Create a new role
            var roleDto = new RoleDto
            {
                Name = "TestRole"
            };

            var roleResponse = await _client.PostAsJsonAsync("/api/role", roleDto, _jsonOptions);
            roleResponse.EnsureSuccessStatusCode();
            var createdRole = await roleResponse.Content.ReadFromJsonAsync<RoleDto>(_jsonOptions);
            Assert.NotNull(createdRole);

            // 3. Assign the role to the user
            var assignResponse = await _client.PostAsJsonAsync($"/api/user/{createdUser.Id}/roles", 
                new List<string> { createdRole.Name }, _jsonOptions);
            assignResponse.EnsureSuccessStatusCode();

            // 4. Verify the role assignment
            var rolesResponse = await _client.GetAsync($"/api/user/{createdUser.Id}/roles");
            rolesResponse.EnsureSuccessStatusCode();
            var userRoles = await rolesResponse.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
            Assert.NotNull(userRoles);
            Assert.Contains(createdRole.Name, userRoles);

            // 5. Remove the role from the user
            var removeResponse = await _client.DeleteAsync($"/api/user/{createdUser.Id}/roles/{createdRole.Name}");
            removeResponse.EnsureSuccessStatusCode();

            // 6. Verify the role was removed
            var finalRolesResponse = await _client.GetAsync($"/api/user/{createdUser.Id}/roles");
            finalRolesResponse.EnsureSuccessStatusCode();
            var finalUserRoles = await finalRolesResponse.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
            Assert.NotNull(finalUserRoles);
            Assert.DoesNotContain(createdRole.Name, finalUserRoles);

            // 7. Clean up - delete the role
            var deleteRoleResponse = await _client.DeleteAsync($"/api/role/{createdRole.Id}");
            deleteRoleResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task RequirementHierarchyWorkflow()
        {
            // 1. Create a parent requirement
            var parentRequirement = new RequirementDto
            {
                Title = "Parent Requirement",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Parent requirement for hierarchy test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var parentResponse = await _client.PostAsJsonAsync("/api/requirement", parentRequirement, _jsonOptions);
            parentResponse.EnsureSuccessStatusCode();
            var createdParent = await parentResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(createdParent);

            // 2. Create child requirements
            var childRequirement1 = new RequirementDto
            {
                Title = "Child Requirement 1",
                Type = RequirementType.PRS,
                Status = RequirementStatus.Draft,
                Description = "First child requirement",
                ParentId = createdParent.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var child1Response = await _client.PostAsJsonAsync("/api/requirement", childRequirement1, _jsonOptions);
            child1Response.EnsureSuccessStatusCode();
            var createdChild1 = await child1Response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(createdChild1);
            Assert.Equal(createdParent.Id, createdChild1.ParentId);

            var childRequirement2 = new RequirementDto
            {
                Title = "Child Requirement 2",
                Type = RequirementType.SRS,
                Status = RequirementStatus.Draft,
                Description = "Second child requirement",
                ParentId = createdParent.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var child2Response = await _client.PostAsJsonAsync("/api/requirement", childRequirement2, _jsonOptions);
            child2Response.EnsureSuccessStatusCode();
            var createdChild2 = await child2Response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(createdChild2);
            Assert.Equal(createdParent.Id, createdChild2.ParentId);

            // 3. Verify the hierarchy by listing all requirements
            var allRequirementsResponse = await _client.GetAsync("/api/requirement");
            allRequirementsResponse.EnsureSuccessStatusCode();
            var allRequirements = await allRequirementsResponse.Content.ReadFromJsonAsync<List<RequirementDto>>(_jsonOptions);
            Assert.NotNull(allRequirements);

            var parent = allRequirements.FirstOrDefault(r => r.Id == createdParent.Id);
            var child1 = allRequirements.FirstOrDefault(r => r.Id == createdChild1.Id);
            var child2 = allRequirements.FirstOrDefault(r => r.Id == createdChild2.Id);

            Assert.NotNull(parent);
            Assert.NotNull(child1);
            Assert.NotNull(child2);
            Assert.Null(parent.ParentId);
            Assert.Equal(parent.Id, child1.ParentId);
            Assert.Equal(parent.Id, child2.ParentId);
        }
    }
}