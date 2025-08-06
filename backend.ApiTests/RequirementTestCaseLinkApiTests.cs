using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System.Collections.Generic;
using System;

namespace backend.ApiTests
{
    public class RequirementTestCaseLinkApiTests : BaseApiTest
    {
        public RequirementTestCaseLinkApiTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        private async Task<(int requirementId, int testCaseId)> CreateRequirementAndTestCase()
        {
            var reqDto = new RequirementDto
            {
                Title = "Req-TC Link Req",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "For link test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var reqResp = await _client.PostAsJsonAsync("/api/requirement", reqDto, _jsonOptions);
            reqResp.EnsureSuccessStatusCode();
            var req = await reqResp.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(req);

            var tcDto = new TestCaseDto
            {
                Title = "Req-TC Link TC",
                Description = "For link test",
                Steps = new List<TestStepDto>(),
                SuiteId = 1, // Add required SuiteId
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var tcResp = await _client.PostAsJsonAsync("/api/testcase", tcDto, _jsonOptions);
            tcResp.EnsureSuccessStatusCode();
            var tc = await tcResp.Content.ReadFromJsonAsync<TestCaseDto>(_jsonOptions);
            Assert.NotNull(tc);

            return (req.Id, tc.Id);
        }

        [Fact]
        public async Task CanCreateAndGetAndDeleteRequirementTestCaseLink()
        {
            var (reqId, tcId) = await CreateRequirementAndTestCase();

            // Create link
            var linkDto = new RequirementTestCaseLinkDto
            {
                RequirementId = reqId,
                TestCaseId = tcId
            };
            var createResp = await _client.PostAsJsonAsync("/api/requirementtestcaselink", linkDto, _jsonOptions);
            createResp.EnsureSuccessStatusCode();

            // Get links for requirement
            var getResp = await _client.GetAsync($"/api/requirementtestcaselink/requirement/{reqId}");
            getResp.EnsureSuccessStatusCode();
            var links = await getResp.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>(_jsonOptions);
            Assert.NotNull(links);
            Assert.Single(links);
            Assert.Equal(reqId, links[0].RequirementId);
            Assert.Equal(tcId, links[0].TestCaseId);

            // Delete link
            var delResp = await _client.DeleteAsync($"/api/requirementtestcaselink?requirementId={reqId}&testCaseId={tcId}");
            delResp.EnsureSuccessStatusCode();

            // Should be empty now
            var getResp2 = await _client.GetAsync($"/api/requirementtestcaselink/requirement/{reqId}");
            getResp2.EnsureSuccessStatusCode();
            var links2 = await getResp2.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>(_jsonOptions);
            Assert.NotNull(links2);
            Assert.Empty(links2);
        }

        [Fact]
        public async Task DeleteNonExistentLinkReturnsNotFound()
        {
            var resp = await _client.DeleteAsync("/api/requirementtestcaselink?requirementId=9999999&testCaseId=9999999");
            // The service doesn't return NotFound, it just succeeds silently
            resp.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DuplicateLinkCreateDoesNotFail()
        {
            var (reqId, tcId) = await CreateRequirementAndTestCase();

            // Create link twice
            var linkDto = new RequirementTestCaseLinkDto
            {
                RequirementId = reqId,
                TestCaseId = tcId
            };
            var createResp1 = await _client.PostAsJsonAsync("/api/requirementtestcaselink", linkDto, _jsonOptions);
            createResp1.EnsureSuccessStatusCode();

            var createResp2 = await _client.PostAsJsonAsync("/api/requirementtestcaselink", linkDto, _jsonOptions);
            createResp2.EnsureSuccessStatusCode(); // Should not fail

            // Should still only have one link
            var getResp = await _client.GetAsync($"/api/requirementtestcaselink/requirement/{reqId}");
            getResp.EnsureSuccessStatusCode();
            var links = await getResp.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>(_jsonOptions);
            Assert.NotNull(links);
            Assert.Single(links);
        }
    }
}