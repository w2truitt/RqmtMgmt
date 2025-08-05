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
    public class RequirementTestCaseLinkApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RequirementTestCaseLinkApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
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
            var reqResp = await _client.PostAsJsonAsync("/api/requirement", reqDto);
            reqResp.EnsureSuccessStatusCode();
            var req = await reqResp.Content.ReadFromJsonAsync<RequirementDto>();
            Assert.NotNull(req);

            var tcDto = new TestCaseDto
            {
                Title = "Req-TC Link TC",
                Description = "For link test",
                Steps = new List<TestStepDto>(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var tcResp = await _client.PostAsJsonAsync("/api/testcase", tcDto);
            tcResp.EnsureSuccessStatusCode();
            var tc = await tcResp.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(tc);

            return (req.Id, tc.Id);
        }

        [Fact]
        public async Task CanCreateAndGetAndDeleteRequirementTestCaseLink()
        {
            var (requirementId, testCaseId) = await CreateRequirementAndTestCase();

            // Create link
            var linkDto = new RequirementTestCaseLinkDto
            {
                RequirementId = requirementId,
                TestCaseId = testCaseId
            };
            var postResp = await _client.PostAsJsonAsync("/api/requirementtestcaselink", linkDto);
            postResp.EnsureSuccessStatusCode();

            // Get by requirement
            var getByReqResp = await _client.GetAsync($"/api/requirementtestcaselink/requirement/{requirementId}");
            getByReqResp.EnsureSuccessStatusCode();
            var linksByReq = await getByReqResp.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>();
            Assert.NotNull(linksByReq);
            Assert.Contains(linksByReq, l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);

            // Get by test case
            var getByTcResp = await _client.GetAsync($"/api/requirementtestcaselink/testcase/{testCaseId}");
            getByTcResp.EnsureSuccessStatusCode();
            var linksByTc = await getByTcResp.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>();
            Assert.NotNull(linksByTc);
            Assert.Contains(linksByTc, l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);

            // Delete link
            var delResp = await _client.DeleteAsync($"/api/requirementtestcaselink?requirementId={requirementId}&testCaseId={testCaseId}");
            delResp.EnsureSuccessStatusCode();

            // Should not find link anymore
            var getByReqResp2 = await _client.GetAsync($"/api/requirementtestcaselink/requirement/{requirementId}");
            getByReqResp2.EnsureSuccessStatusCode();
            var linksByReq2 = await getByReqResp2.Content.ReadFromJsonAsync<List<RequirementTestCaseLinkDto>>();
            Assert.NotNull(linksByReq2);
            Assert.DoesNotContain(linksByReq2, l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);
        }

        [Fact]
        public async Task DeleteNonExistentLinkReturnsNotFound()
        {
            var resp = await _client.DeleteAsync($"/api/requirementtestcaselink?requirementId=9999999&testCaseId=9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DuplicateLinkCreateDoesNotFail()
        {
            var (requirementId, testCaseId) = await CreateRequirementAndTestCase();
            var linkDto = new RequirementTestCaseLinkDto
            {
                RequirementId = requirementId,
                TestCaseId = testCaseId
            };
            var postResp1 = await _client.PostAsJsonAsync("/api/requirementtestcaselink", linkDto);
            postResp1.EnsureSuccessStatusCode();
            var postResp2 = await _client.PostAsJsonAsync("/api/requirementtestcaselink", linkDto);
            postResp2.EnsureSuccessStatusCode();
        }
    }
}
